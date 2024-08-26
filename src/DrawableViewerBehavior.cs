using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms.Integration;

#if AUTOCAD2015_TO_2024
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.DatabaseServices;
#elif GSTARCAD2024_TO_2025
using Gssoft.Gscad.GraphicsInterface;
using Gssoft.Gscad.DatabaseServices;
#elif GSTARCAD2017_TO_2023
using GrxCAD.GraphicsInterface;
using GrxCAD.DatabaseServices;
#endif

#if AUTOCAD2015_TO_2024
namespace Sharper.AutoCAD.DrawableViewer
#else
namespace Sharper.GstarCAD.Extensions
#endif
{
    /// <summary>
    /// 用于WPF控件的附加属性行为类，通过对 <see cref="WindowsFormsHost"/> 应用附加属性行为，间接对 <see cref="DrawableViewer"/> 进行数据绑定
    /// </summary>
    public static class DrawableViewerBehavior
    {
        /// <summary>
        /// 可绑定的绘图对象的属性
        /// </summary>
        public static readonly DependencyProperty DrawableObjectProperty =
            DependencyProperty.RegisterAttached("DrawableObject", typeof(Drawable), typeof(DrawableViewerBehavior),
                new FrameworkPropertyMetadata(OnDrawableObjectChanged)
                {
                    DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, BindsTwoWayByDefault = false
                });

        /// <summary>
        /// 绘图对象更改的事件处理器
        /// </summary>
        /// <param name="d">被绑定的 <see cref="WindowsFormsHost"/> 对象</param>
        /// <param name="e">绑定事件</param>
        /// <exception cref="NotSupportedException"></exception>
        private static void OnDrawableObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is WindowsFormsHost host) || !(host.Child is DrawableViewer viewer))
                return;

            if (e.Property != DrawableObjectProperty)
                return;

            viewer.EraseAll();
            if (!(e.NewValue is Drawable drawable))
            {
                viewer.Invalidate();
                return;
            }

            switch (drawable)
            {
                case BlockTableRecord blockTableRecord:
                    viewer.Add(blockTableRecord);
                    break;
                case Entity entity:
                    viewer.Add(entity);
                    break;
                default:
                    throw new NotSupportedException($"不支持的预览对象: {drawable.GetRXClass().Name}");
            }

            if (GetAutoZoomingWhenDrawableChanged(d))
                viewer.ZoomExtents();
            viewer.SetMouseAction(GetMouseActionResponse(d));

            viewer.Invalidate();
        }

        /// <summary>
        /// 绘图对象改变时是否进行视图自适应缩放的属性
        /// </summary>
        public static readonly DependencyProperty AutoZoomingWhenDrawableChangedProperty =
            DependencyProperty.RegisterAttached("AutoZoomingWhenDrawableChanged", typeof(bool),
                typeof(DrawableViewerBehavior), new PropertyMetadata(true));

        /// <summary>
        /// 设置绘图对象
        /// </summary>
        /// <param name="d">被绑定的 <see cref="WindowsFormsHost"/> 对象</param>
        /// <param name="drawable">绘图对象</param>
        public static void SetDrawableObject(DependencyObject d, Drawable drawable)
        {
            d.SetValue(DrawableObjectProperty, drawable);
        }

        /// <summary>
        /// 获取绘图对象
        /// </summary>
        /// <param name="d">被绑定的 <see cref="WindowsFormsHost"/> 对象</param>
        /// <returns>绘图对象</returns>
        public static Drawable GetDrawableObject(DependencyObject d)
        {
            return (Drawable)d.GetValue(DrawableObjectProperty);
        }

        /// <summary>
        /// 设置绘图对象改变时是否进行视图自适应缩放
        /// </summary>
        /// <param name="d">被绑定的 <see cref="WindowsFormsHost"/> 对象</param>
        /// <param name="autoZoomingWhenDrawableChanged">是否自适应缩放</param>
        public static void SetAutoZoomingWhenDrawableChanged(DependencyObject d, bool autoZoomingWhenDrawableChanged)
        {
            d.SetValue(AutoZoomingWhenDrawableChangedProperty, autoZoomingWhenDrawableChanged);
        }

        /// <summary>
        /// 获取绘图对象改变时是否进行视图自适应缩放
        /// </summary>
        /// <param name="d">被绑定的 <see cref="WindowsFormsHost"/> 对象</param>
        /// <returns>是否自适应缩放</returns>
        public static bool GetAutoZoomingWhenDrawableChanged(DependencyObject d)
        {
            return (bool)d.GetValue(AutoZoomingWhenDrawableChangedProperty);
        }

        /// <summary>
        /// 绘图对象是否响应鼠标事件
        /// </summary>
        public static readonly DependencyProperty MouseActionProperty =
            DependencyProperty.RegisterAttached("MouseAction", typeof(bool),
                typeof(DrawableViewerBehavior), new PropertyMetadata(true));

        /// <summary>
        /// 设置绘图对象改变时是否进行视图自适应缩放
        /// </summary>
        /// <param name="d">被绑定的 <see cref="WindowsFormsHost"/> 对象</param>
        /// <param name="autoZoomingWhenDrawableChanged">是否自适应缩放</param>
        public static void SetMouseActionResponse(DependencyObject d, bool mouseAction)
        {
            d.SetValue(MouseActionProperty, mouseAction);
        }

        /// <summary>
        /// 获取绘图对象改变时是否进行视图自适应缩放
        /// </summary>
        /// <param name="d">被绑定的 <see cref="WindowsFormsHost"/> 对象</param>
        /// <returns>是否自适应缩放</returns>
        public static bool GetMouseActionResponse(DependencyObject d)
        {
            return (bool)d.GetValue(MouseActionProperty);
        }
    }
}