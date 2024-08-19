#if AUTOCAD2015_TO_2024
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
#elif GSTARCAD2024_TO_2025
using Gssoft.Gscad.DatabaseServices;
using Gssoft.Gscad.Geometry;
#elif GSTARCAD2017_TO_2023
using GrxCAD.DatabaseServices;
using GrxCAD.Geometry;
#endif

#if AUTOCAD2015_TO_2024
namespace Sharper.AutoCAD.DrawableViewer
#else
namespace Sharper.GstarCAD.Extensions
#endif
{
    /// <summary>
    /// <see cref="Extents3d"/> 的扩展类
    /// </summary>
    internal static class Extents3dExtension
    {
        /// <summary>
        /// 判别包围盒是否有效
        /// </summary>
        /// <param name="extents">包围盒对象</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(this Extents3d extents)
        {
            return extents.MinPoint.X <= extents.MaxPoint.X &&
                   extents.MinPoint.Y <= extents.MaxPoint.Y &&
                   extents.MinPoint.Z <= extents.MaxPoint.Z;
        }

        /// <summary>
        /// 获取包围盒对象的中心点
        /// </summary>
        /// <param name="extents">包围盒对象</param>
        /// <returns>中心点对象</returns>
        public static Point3d GetCenter(this Extents3d extents)
        {
            return new Point3d(extents.MinPoint.X + (extents.MaxPoint.X - extents.MinPoint.X) / 2,
                extents.MinPoint.Y + (extents.MaxPoint.Y - extents.MinPoint.Y) / 2,
                extents.MinPoint.Z + (extents.MaxPoint.Z - extents.MinPoint.Z) / 2);
        }
    }
}