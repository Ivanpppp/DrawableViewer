using System;
using System.Windows.Forms;
using System.Drawing;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.Mime.MediaTypeNames;




#if AUTOCAD2015_TO_2024
using Autodesk.AutoCAD;
using Autodesk.AutoCAD.GraphicsSystem;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using View = Autodesk.AutoCAD.GraphicsSystem.View;
using ErrorStatus = Autodesk.AutoCAD.Runtime.ErrorStatus;
using Exception = Autodesk.AutoCAD.Runtime.Exception;
#elif GSTARCAD2024_TO_2025
using Gssoft.Gscad;
using Gssoft.Gscad.GraphicsSystem;
using Gssoft.Gscad.DatabaseServices;
using Gssoft.Gscad.Geometry;
using Application = Gssoft.Gscad.ApplicationServices.Core.Application;
using View = Gssoft.Gscad.GraphicsSystem.View;
using ErrorStatus = Gssoft.Gscad.Runtime.ErrorStatus;
using Exception = Gssoft.Gscad.Runtime.Exception;
#elif GSTARCAD2017_TO_2023
using GrxCAD.GraphicsSystem;
using GrxCAD.DatabaseServices;
using GrxCAD.Geometry;
using Application = GrxCAD.ApplicationServices.Application;
using View = GrxCAD.GraphicsSystem.View;
using ErrorStatus = GrxCAD.Runtime.ErrorStatus;
using Exception = GrxCAD.Runtime.Exception;
#endif

#if AUTOCAD2015_TO_2024
namespace Sharper.AutoCAD.DrawableViewer
#else
namespace Sharper.GstarCAD.Extensions
#endif
{
    /// <summary>
    /// 基于 Windows Form 的CAD图形可视化控件对象
    /// </summary>
    public class DrawableViewer : Panel
    {
        /// <summary>
        /// CAD绘图系统的设备对象
        /// </summary>
        private readonly Device _device;

        /// <summary>
        /// CAD绘图系统的模型对象
        /// </summary>
        private readonly Model _model;

        /// <summary>
        /// CAD绘图系统的视图对象
        /// </summary>
        private readonly View _view;

        /// <summary>
        /// 可视化图形的包围盒
        /// </summary>
        private Extents3d _extents;

        /// <summary>
        /// 记录初始点位
        /// </summary>
        private Point _startPt;

        /// <summary>
        /// 判断控件是否需要相应鼠标操作
        /// </summary>
        private bool _mouseAction;

        /// <summary>
        /// 鼠标滚轮缩小比例
        /// </summary>
        private readonly double _mouseWheelZoomInScale = 0.9;

        /// <summary>
        /// 鼠标滚轮放大比例
        /// </summary>
        private readonly double _mouseWheelZoomOutScale = 1.1;

        /// <summary>
        /// 默认构造器，对绘图系统的模型、设备和视图进行初始化
        /// </summary>
        public DrawableViewer()
        {
            var manager = Application.DocumentManager.MdiActiveDocument.GraphicsManager;

#if GSTARCAD2024_TO_2025 || AUTOCAD2015_TO_2024
            using (var descriptor = new KernelDescriptor())
            {
                descriptor.addRequirement(UniqueString.Intern("3D Drawing"));
                var kernel = Manager.AcquireGraphicsKernel(descriptor);
                _device = manager.CreateAutoCADDevice(kernel, Handle);
                _model = manager.CreateAutoCADModel(kernel);
                Manager.ReleaseGraphicsKernel(kernel);
            }
#else
            _device = manager.CreateAutoCADDevice(Handle);
            _model = manager.CreateAutoCADModel();
#endif
            _view = new View();
            _device.Add(_view);
            _mouseAction = true;
        }

        /// <summary>
        /// 添加块表记录可视化对象
        /// </summary>
        /// <param name="blockTableRecord">块表记录对象</param>
        /// <exception cref="ArgumentNullException">对象为空</exception>
        /// <exception cref="Exception">对象的包围盒无效</exception>
        public void Add(BlockTableRecord blockTableRecord)
        {
            if (blockTableRecord == null)
                throw new ArgumentNullException(nameof(blockTableRecord));

            var blockExtents = new Extents3d();
            blockExtents.AddBlockExtents(blockTableRecord);
            if (!blockExtents.IsValid())
                throw new Exception(ErrorStatus.NullExtents);

            _extents.AddExtents(blockExtents);
            _view.Add(blockTableRecord, _model);
        }

        /// <summary>
        /// 添加实体可视化对象
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <exception cref="ArgumentNullException">对象为空</exception>
        /// <exception cref="Exception">对象的包围盒无效</exception>
        public void Add(Entity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var extents = entity is BlockReference blockReference
                ? blockReference.GeometryExtentsBestFit()
                : entity.GeometricExtents;
            if (!extents.IsValid())
                throw new Exception(ErrorStatus.NullExtents);

            _extents.AddExtents(extents);
            _view.Add(entity, _model);
        }

        /// <summary>
        /// 删除所有加入的可视化对象
        /// </summary>
        public void EraseAll()
        {
            _view.EraseAll();
            _view.Invalidate();

            _extents = new Extents3d();
        }

        /// <summary>
        /// 缩放至俯视图下的最大范围
        /// </summary>
        public void ZoomExtents()
        {
            var center = _extents.GetCenter();
            double width = _extents.MaxPoint.X - _extents.MinPoint.X;
            double height = _extents.MaxPoint.Y - _extents.MinPoint.Y;
            _view.SetView(center + Vector3d.ZAxis, center, Vector3d.YAxis, 1.05 * width, 1.05 * height);
        }

        /// <summary>
        /// 设置视图的相机参数
        /// </summary>
        /// <param name="position">相机位置</param>
        /// <param name="target">相机目标</param>
        /// <param name="upVector">相机的上向量</param>
        /// <param name="fieldWidth">投影平面的宽度</param>
        /// <param name="fieldHeight">投影平面的高度</param>
        /// <param name="projection">投影方式</param>
        public void SetView(Point3d position, Point3d target, Vector3d upVector,
            double fieldWidth, double fieldHeight, Projection projection = Projection.Parallel)
        {
            _view.SetView(position, target, upVector, fieldWidth, fieldHeight, projection);
        }

        /// <summary>
        /// 可视化图形的包围盒
        /// </summary>
        public Extents3d Extents => _extents;

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _device.Erase(_view);
                _view.Dispose();
                _model.Dispose();
                _device.Dispose();
                _extents = new Extents3d();
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc />
        protected override void OnPaint(PaintEventArgs e)
        {
            _view.Invalidate();
            _view.Update();
            base.OnPaint(e);
        }

        /// <inheritdoc />
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
#if GSTARCAD2024_TO_2025 || AUTOCAD2015_TO_2024
            _device.OnSize(ClientSize);
#else
            _device.OnSize(ClientRectangle);
#endif
            Invalidate();
        }

        /// <summary>
        /// 设置空间是否需要响应鼠标事件
        /// </summary>
        /// <param name="bCapture"></param>
        public void SetMouseAction(bool bCapture)
        {
            _mouseAction = bCapture;
        }


        /// <inheritdoc />
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (!_mouseAction) return;
            if (_view != null)
            {
                if(e.Button == MouseButtons.Right) ResetView();
            }
        }


        /// <inheritdoc />
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!_mouseAction) return;
            if (_view != null)
            {
                if (e.Button == MouseButtons.Middle)
                {
                    PanView(e.Location);
                }
                if (e.Button == MouseButtons.Left)
                {
                    OrbitView(e.Location);
                }
                _view.Invalidate();
                _view.Update();
                _startPt = e.Location;
            }
        }

        /// <inheritdoc />
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (!_mouseAction) return;
            if (e.Delta > 0)
            {
                _view?.Zoom(_mouseWheelZoomOutScale);
            }
            else
            {
                _view?.Zoom(_mouseWheelZoomInScale);
            }
            _view.Invalidate();
            _view.Update();
        }

        /// <summary>
        /// 将View重置为俯视图
        /// </summary>
        private void ResetView()
        {
            var pos = new Point3d(_extents.MaxPoint.X + (_extents.MinPoint.X - _extents.MaxPoint.X) / 2, _extents.MaxPoint.Y + (_extents.MinPoint.Y - _extents.MaxPoint.Y) / 2, 1);
            var tar = new Point3d(pos.X, pos.Y, 0);
            _view.SetView(pos, tar, new Vector3d(0, 1, 0), this.Width, this.Height);
            _view.ZoomExtents(_extents.MinPoint, _extents.MaxPoint);
            _view.Zoom(0.95);
            _view.Invalidate();
            _view.Update();
        }

        /// <summary>
        /// 二维平面下移动视角
        /// </summary>
        /// <param name="pt">新的点位置</param>
        private void PanView(System.Drawing.Point pt)
        {
            var vec = new Vector3d(-(pt.X - _startPt.X), pt.Y - _startPt.Y, 0);
            vec.TransformBy(_view.ViewingMatrix * _view.WorldToDeviceMatrix.Inverse());
            _view.Dolly(vec);
        }

        /// <summary>
        /// 三维控件旋转View视角
        /// </summary>
        /// <param name="pt">新的点位置</param>
        private void OrbitView(System.Drawing.Point pt)
        {
            double Half_Pi = 1.570796326795;
#if GSTARCAD2024_TO_2025 || AUTOCAD2015_TO_2024

            System.Drawing.Rectangle view_rect = ClientRectangle;
#else
            System.Drawing.Rectangle view_rect = _view.Viewport;
#endif
            int nViewportX = (view_rect.Right - view_rect.Left) + 1;
            int nViewportY = (view_rect.Bottom - view_rect.Top) + 1;
            int centerX = (int)(nViewportX / 2.0f + view_rect.Left);
            int centerY = (int)(nViewportY / 2.0f + view_rect.Top);
            double radius = System.Math.Min(nViewportX, nViewportY) * 0.4f;
            Vector3d last_vector = new Vector3d((_startPt.X - centerX) / radius,
                -(_startPt.Y - centerY) / radius,
                0.0);
            if (last_vector.LengthSqrd > 1.0)     // outside the radius
            {
                double x = last_vector.X / last_vector.Length;
                double y = last_vector.Y / last_vector.Length;
                double z = last_vector.Z / last_vector.Length;
                last_vector = new Vector3d(x, y, z);
            }
            else
            {
                double x = last_vector.X;
                double y = last_vector.Y;
                double z = System.Math.Sqrt(1.0 - last_vector.X * last_vector.X - last_vector.Y * last_vector.Y);
                last_vector = new Vector3d(x, y, z);
            }
            Vector3d new_vector = new Vector3d((pt.X - centerX) / radius, -(pt.Y - centerY) / radius, 0.0);

            if (new_vector.LengthSqrd > 1.0)     // outside the radius
            {
                double x = new_vector.X / new_vector.Length;
                double y = new_vector.Y / new_vector.Length;
                double z = new_vector.Z / new_vector.Length;
                new_vector = new Vector3d(x, y, z);

            }
            else
            {
                double x = new_vector.X;
                double y = new_vector.Y;
                double z = System.Math.Sqrt(1.0 - new_vector.X * new_vector.X - new_vector.Y * new_vector.Y);
                new_vector = new Vector3d(x, y, z);
            }
            Vector3d rotation_vector = last_vector;
            rotation_vector = rotation_vector.CrossProduct(new_vector); 
            Vector3d work_vector = rotation_vector;
            work_vector = new Vector3d(work_vector.X, work_vector.Y, 0.0f);
            double roll_angle = System.Math.Atan2(work_vector.X, work_vector.Y);
            double length = rotation_vector.Length;
            double orbit_y_angle = (length != 0.0) ? System.Math.Acos(rotation_vector.Z / length) + Half_Pi : Half_Pi;                   // represents inverse cosine of the dot product of the
            if (length > 1.0f) length = 1.0f;

            double rotation_angle = System.Math.Asin(length);

            // perform view manipulations
            _view.Roll(roll_angle);                 // 1: roll camera to make up vector coincident with rotation vector
            _view.Orbit(0.0f, orbit_y_angle);       // 2: orbit along y to make up vector parallel with rotation vector
            _view.Orbit(rotation_angle, 0.0f);      // 3: orbit along x by rotation angle
            _view.Orbit(0.0f, -orbit_y_angle);      // 4: orbit along y by the negation of 2
            _view.Roll(-roll_angle);                // 5: roll camera by the negation of 1
        }
    }
}