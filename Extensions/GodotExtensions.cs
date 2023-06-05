//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.

//  Note: A place to put Godot related extension methods.
namespace WFLib;
public static class GodotExtensions
{
    public static Vector2 CalcSize(this LineEdit le, int charCount)
    {
        int fs = le.GetThemeFontSize("font_size");
        var font = le.GetThemeFont("font");
        var sb = le.GetThemeStylebox("normal");
        float marginWidth = sb.ContentMarginLeft + sb.ContentMarginRight;
        float marginHeight = sb.ContentMarginTop + sb.ContentMarginBottom;
        Span<char> text = stackalloc char[charCount];
        for (int i = 0; i < charCount; i++)
        {
            text[i] = '9';
        }
        var ls = font.GetStringSize(text.ToString(), fontSize: fs);
        ls.X += marginWidth;
        ls.Y += marginHeight;
        return ls;
    }
    public static Vector2I AsVector2I(this Godot.Vector2 v)
    {
        return new Vector2I((int)v.X, (int)v.Y);
    }
    public static Vector3 Forward(this Transform3D t)
    {
        return -t.Basis.Z;
    }
    public static Vector3 Back(this Transform3D t)
    {
        return t.Basis.Z;
    }
    public static Vector3 Left(this Transform3D t)
    {
        return -t.Basis.X;
    }
    public static Vector3 Right(this Transform3D t)
    {
        return t.Basis.X;
    }
    public static Vector3 Up(this Transform3D t)
    {
        return t.Basis.Y;
    }
    public static Vector3 Down(this Transform3D t)
    {
        return -t.Basis.Y;
    }
    //public static void WritePB(this Godot.FileAccess file, PacketBuffer pb)
    //{
    //	if (pb.PacketCount > byte.MaxValue) throw new System.Exception("To many packets");
    //	file.Store32((uint)pb.PacketSize);
    //	file.Store32((uint)pb.PacketCount);
    //	pb.Process(WritePacketBuf);
    //	void WritePacketBuf(byte[] buf, int size)
    //	{
    //		file.StoreBuffer(buf);
    //	}
    //}
    //public static void ReadPB(this Godot.FileAccess file, PacketBuffer pb)
    //{
    //	pb.Clear();
    //	int packetSize = (int) file.Get32();
    //	int npackets = (int) file.Get32();
    //	if (packetSize < pb.PacketSize)
    //	{
    //           pb.Resize(packetSize);
    //	}
    //	for(int i=0;i<npackets;i++)
    //	{
    //		pb.BlockCopy(file.GetBuffer(packetSize),0,packetSize);
    //	}
    //}
    //public static void CornerRadius(this StyleBoxFlat sb, UIMargin cr)
    //   {
    //       sb.CornerRadiusTopLeft = cr.Left;
    //       sb.CornerRadiusTopRight = cr.Right;
    //       sb.CornerRadiusBottomRight = cr.Top;
    //       sb.CornerRadiusBottomLeft = cr.Bottom;
    //   }
    //   public static void ExpandMargin(this StyleBoxFlat sb, UIMargin m)
    //   {
    //       sb.ExpandMarginLeft = m.Left;
    //       sb.ExpandMarginRight = m.Right;
    //       sb.ExpandMarginTop = m.Top;
    //       sb.ExpandMarginBottom = m.Bottom;
    //   }
    //   public static void BorderWidth(this StyleBoxFlat sb, UIMargin m)
    //   {
    //       sb.BorderWidthLeft = m.Left;
    //       sb.BorderWidthRight = m.Right;
    //       sb.BorderWidthTop = m.Top;
    //       sb.BorderWidthBottom = m.Bottom;
    //   }
    //   public static void ContentMargin(this StyleBoxFlat sb, UIMargin m)
    //   {
    //       sb.ContentMarginLeft = m.Left;
    //       sb.ContentMarginRight = m.Right;
    //       sb.ContentMarginTop = m.Top;
    //       sb.ContentMarginBottom = m.Bottom;
    //   }
    public static Quaternion GetRotationTo(this MeshInstance3D from, Vector3 to)
    {
        Transform3D t = new Transform3D(from.GlobalTransform.Basis, from.GlobalPosition);
        var curRot = t.Basis.GetRotationQuaternion();
        var targetPos = from.GlobalPosition + (to * 10);
        t = t.LookingAt(targetPos, Vector3.Up);
        var targetRot = t.Basis.GetRotationQuaternion();
        targetRot = targetRot - curRot;
        return targetRot;
    }
    //  Note: Move the pivot point to be the calculated center of the mesh
    public static void CenterThePivot(this MeshInstance3D mi)
    {
        ArrayMesh mesh = new ArrayMesh();
        HashSet<Vector3> hashSet = new HashSet<Vector3>();
        int scount = mi.Mesh.GetSurfaceCount();
        double x = 0;
        double y = 0;
        double z = 0;
        int count = 0;
        for (int si = 0; si < scount; si++)
        {
            var arrays = mi.Mesh.SurfaceGetArrays(si);
            var verts = arrays[0].AsVector3Array();
            for (int i = 0; i < verts.Length; i++)
            {
                var v = verts[i];
                if (hashSet.Contains(v)) continue; //ignore duplicate verts
                hashSet.Add(v);
                x += v.X;
                y += v.Y;
                z += v.Z;
                count++;
            }
        }
        x = x / count;
        y = y / count;
        z = z / count;
        Vector3 pivotPos = new Vector3((float)x, (float)y, (float)z); // calculated mid point
        //rebuild the mesh with the new midPoint at Vector3.Zero
        for (int si = 0; si < scount; si++)
        {
            var arrays = mi.Mesh.SurfaceGetArrays(si);
            var verts = arrays[0].AsVector3Array();
            for (int i = 0; i < verts.Length; i++)
            {
                verts[i] = verts[i] - pivotPos; //turns the midpoint into Vector3.Zero
            }
            mesh.AddSurfaceFromArrays(Godot.Mesh.PrimitiveType.Triangles, arrays);
            mesh.SurfaceSetMaterial(si, mi.Mesh.SurfaceGetMaterial(si));
        }
        mi.Mesh = mesh;
        mi.Translate(pivotPos); //leave mesh in same physival space, only the pivot point changes
    }
}