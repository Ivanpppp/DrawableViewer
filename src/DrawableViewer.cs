using System;
using System.Windows.Forms;
using System.Drawing;

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
        /// 记录上一个鼠标坐标点位
        /// </summary>
        private Point _lastPoint;

        /// <summary>
        /// 控件能否响应鼠标操作
        /// </summary>
        public bool CanMouseOperation { get; set; } = true;

        /// <summary>
        /// 鼠标滚轮缩放比例，放大则为<code>1+<see cref="MouseZoomDelta"/></code>，缩小则为<code>1-<see cref="MouseZoomDelta"/></code>
        /// </summary>
        private const double MouseZoomDelta = 0.1;

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
            SetView(center + Vector3d.ZAxis, center, Vector3d.YAxis, 1.05 * width, 1.05 * height);
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

        /// <inheritdoc />
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (!CanMouseOperation)
                return;
            if (e.Button != MouseButtons.Right || !ClientRectangle.Contains(e.Location))
                return;

            ZoomExtents();
            _view.Invalidate();
            _view.Update();
        }


        /// <inheritdoc />
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!CanMouseOperation)
                return;

            switch (e.Button)
            {
                case MouseButtons.Middle:
                    PanView(e.Location);
                    _view.Invalidate();
                    _view.Update();
                    break;
                case MouseButtons.Left:
                    OrbitView(e.Location);
                    _view.Invalidate();
                    _view.Update();
                    break;
            }

            _lastPoint = e.Location;
        }

        /// <inheritdoc />
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (!CanMouseOperation)
                return;

            _view.Zoom(e.Delta > 0 ? 1 + MouseZoomDelta : 1 - MouseZoomDelta);
            _view.Invalidate();
            _view.Update();
        }

        /// <summary>
        /// 二维平面下移动视角
        /// </summary>
        /// <param name="newPoint">新的点位置</param>
        private void PanView(Point newPoint)
        {
#if AUTOCAD2015_TO_2024
            var moveVector = new Vector3d(-(newPoint.X - _lastPoint.X), newPoint.Y - _lastPoint.Y, 0);
#else
            var moveVector = new Vector3d(-(newPoint.X - _lastPoint.X), -(newPoint.Y - _lastPoint.Y), 0);
#endif
            moveVector = moveVector.TransformBy(_view.ViewingMatrix * _view.WorldToDeviceMatrix.Inverse());
            _view.Dolly(moveVector);
        }

        /// <summary>
        /// 三维控件旋转View视角
        /// </summary>
        /// <param name="newPoint">新的点位置</param>
        private void OrbitView(Point newPoint)
        {
            double centerX = ClientRectangle.Width / 2.0 + ClientRectangle.Left;
            double centerY = ClientRectangle.Height / 2.0 + ClientRectangle.Top;
            double radius = Math.Min(ClientRectangle.Width, ClientRectangle.Height) * 0.4f;

            Vector3d lastVector =
                GetValidVector(new Vector3d((_lastPoint.X - centerX) / radius, -(_lastPoint.Y - centerY) / radius,
                    0.0));
            Vector3d newVector =
                GetValidVector(new Vector3d((newPoint.X - centerX) / radius, -(newPoint.Y - centerY) / radius, 0.0));

            Vector3d rotationVector = lastVector.CrossProduct(newVector);
            Vector3d workingVector = new Vector3d(rotationVector.X, rotationVector.Y, 0.0);
            double rollAngle = Math.Atan2(workingVector.X, workingVector.Y);
            double orbitYAngle = rotationVector.Length != 0.0
                ? Math.Acos(rotationVector.Z / rotationVector.Length) + Math.PI / 2
                : Math.PI;

            double rotationAngle = Math.Asin(Math.Min(rotationVector.Length, 1.0));

            // perform view manipulations
            // 1: roll camera to make up vector coincident with rotation vector
            _view.Roll(rollAngle);
            // 2: orbit along y to make up vector parallel with rotation vector
            _view.Orbit(0.0, orbitYAngle);
            // 3: orbit along x by rotation angle
            _view.Orbit(rotationAngle, 0.0);
            // 4: orbit along y by the negation of 2
            _view.Orbit(0.0, -orbitYAngle);
            // 5: roll camera by the negation of 1
            _view.Roll(-rollAngle);
        }

        /// <summary>
        /// 获取有效的 <see cref="Vector3d"/> 对象
        /// </summary>
        /// <param name="vector">原始数据</param>
        /// <returns>处理后的数据</returns>
        private static Vector3d GetValidVector(Vector3d vector)
        {
            if (vector.LengthSqrd > 1.0)
            {
                // outside the radius
                vector = vector.GetNormal();
            }
            else
            {
                double z = Math.Sqrt(1.0 - vector.X * vector.X - vector.Y * vector.Y);
                vector = new Vector3d(vector.X, vector.Y, z);
            }

            return vector;
        }
    }
}