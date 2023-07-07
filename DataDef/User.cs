//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
namespace WFLib;
public enum UserRole { Guest, User, Admin, EditUser, Developer}
public partial class RolesData : Data
{
    public UserRole Role;
    public bool Active;
}
public partial class User : Record
{
    [K] public string GUID;
    public string NickName;
    public string PasswordHash;
    public string RecoverHash;
    public RolesData.DataField Roles;
}
public partial class User 
{ 
    [X] public RSA RSA = null;
    public void Load()
    {
        if (!File.Exists(UserDataFile))
        {
            SaveNewKeyFile(this);
            return;
        }


        using (var zipFile = new WFZip())
        {
            if (zipFile.Open(UserDataFile, "test") == WFZip.Status.Ok)
            {
                SerializationBuffer sb = SerializationBuffer.Rent();
                var status = zipFile.ReadByteArray("User.dat", sb.Buf);
                if (status == WFZip.Status.Ok)
                {
                    ReadFromBuf(sb);
                    //Util.Deserialize(this,sb);
                    Util.ReadByGUIDKey(this);
                }
                //sb.Return();
                ByteArray privKey = ByteArray.Rent();
                status = zipFile.ReadByteArray("Private.key", privKey);
                RSA = new RSA(privKey);
                privKey.Return();
            }
            zipFile.Close();
        }
    }
    static void SaveNewKeyFile(User data)
    {
        data.GUID = Guid.NewGuid().ToString();
        Util.Add(data, true);
        data.RSA = new();
        SerializationBuffer sb = SerializationBuffer.Rent();
        data.WriteToBuf(sb);
        //Util.Serialize(data, sb);
        using (var zipFile = new WFZipWriter(UserDataFile, "test"))
        {
            var status = zipFile.WriteByteArray("User.dat", sb.Buf);
            var privKey = data.RSA.GetPrivateKey();
            //we only need to save the private key as it also contains the public key
            status = zipFile.WriteByteArray("Private.key", privKey);
            zipFile.Close();
            privKey.Return();
        }
    }
    private static string UserDataFile
    {
        get
        {
            string ud = Directories.UserData;
            ud = Path.Combine(ud, "User.zip");
            return ud;
        }
    }
    public bool MakeGuest()
    {
        if (Equals(this, GuestUser)) return false;
        GuestUser.CopyTo(this);
        return true;
    }
    static User GuestUser;
    static User()
    {
        GuestUser = Util.RentRecord();
        if (Util.Read(GuestUser, 1) != Status.Ok)
        {
            GuestUser.ID = 1;
            GuestUser.EditByID = 0;
            GuestUser.GUID = "N/A";
            GuestUser.NickName = "Guest";
            GuestUser.PasswordHash = "";
            GuestUser.RecoverHash = "";
            GuestUser.RSA = null;
            //Roles is a DataField, a DataField always has at least one Data object
            GuestUser.Roles.Data.Role =  UserRole.Guest;
            GuestUser.Roles.Data.Active = true;
            Util.Add(GuestUser);
        }
    }
}