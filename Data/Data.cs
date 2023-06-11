//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
namespace WFLib;
public abstract class Data : IDisposable
{
    internal int _id = 0;
    public abstract int FieldCount { get; }
    protected virtual void OnBaseConstruct() { }
    public virtual void OnInitialize() { }
    public virtual void OnLoad() { }
    public virtual void OnClear() { }
    public abstract void Clear();
    public abstract void Init();
    public abstract void WriteToBuf(SerializationBuffer buf,bool append=false);
    public abstract void ReadFromBuf(SerializationBuffer buf, int maxField);
    public abstract void ReadFromBuf(SerializationBuffer buf);
    public abstract void CopyTo(Data to);
    public abstract void FieldCopyTo(Data to, int field);
    public abstract bool FieldIsDefault(int field);
    public abstract bool FieldIsEqual(Data to, int field);
    public abstract string FieldAsString(int field);
    public abstract Object FieldAsObject(int field);
    public abstract void FieldFromObject(Object o, int field);
    public abstract void FieldFromString(string s, int field);
    public abstract string FieldName(int field);
    public abstract string FieldLabel(int field);
    public abstract void FieldLabelSet(int field, string label);
    public abstract string FieldColumnLabel(int field);
    public abstract void FieldColumnLabelSet(int field, string label);
    public abstract Type FieldType(int field);
    public abstract string[] StaticTypeNames { get; }
    public string FieldTypeName(int field)
    {
        if (field < 0 || field >= FieldCount) return "";
        return StaticTypeNames[field];
    }
    public abstract void FieldMinSet(int field, object min);
    public abstract object FieldMin(int field);
    public abstract void FieldMaxSet(int field, object max);
    public abstract object FieldMax(int field);
    public abstract int FieldIdFromName(string name);
    public abstract void FieldAsKey(int field, SerializationBuffer sb, int maxSize);
    //public abstract bool FieldValidate(int field);
    //public abstract string FieldValidationMessage(int field);
    private IEditHelper[] _editHelpers = null;
    private void EditHelpersInit()
    {
        if (_editHelpers == null)
        {
            _editHelpers = new IEditHelper[FieldCount];
        }
    }
    public IEditHelper FieldEditHelper(int field)
    {
        EditHelpersInit();
        return _editHelpers[field];
    }
    public void FieldEditHelperSet(int field, IEditHelper editHelper)
    {
        EditHelpersInit();
        if ( _editHelpers[field] != null)
        {
            _editHelpers[field].Dispose();
        }
        _editHelpers[field] = editHelper;
    }
    public void VFRequired(int field)
    {
        FieldValidationFuncAdd(field, (Data d,int f, out string msg) => 
        {
            msg = "";
            if (d.FieldIsDefault(field))
            {
                msg = "Required";
                return false;
            }
            return true;
        });
    }
    public void VFPassword(int field)
    {
        VFRequired(field);
        FieldValidationFuncAdd(field, (Data d, int f, out string msg) =>
        {
            msg = "";
            if (d.FieldAsString(field).Length < 8)
            {
                msg = "Password must be at least 8 characters";
                return false;
            }
            return true;
        });
    }
    public void VFConfirmPassword(int confirmPasswordField, int passwordField )
    {
        VFPassword(confirmPasswordField);
        FieldValidationFuncAdd(confirmPasswordField , (Data d,int f, out string msg) =>
        {
            msg = "";
            if (d.FieldAsString(confirmPasswordField) != d.FieldAsString(passwordField))
            {
                msg = "does not match password";
                return false;
            }
            return true;
        });
    }

    static Func<Data, string>[] HtmlHelpers = new Func<Data, string>[15];
    public abstract Func<Data, string>[] StaticHtmlHelpers { get; }
    public bool FieldHasHtmlHelper(int field)
    {
        if (field < 0 || field >= FieldCount) return false;
        return StaticHtmlHelpers[field] != null;
    }
    public string FieldAsHtml(int field)
    {
        if (!FieldHasHtmlHelper(field)) return FieldAsString(field);
        return StaticHtmlHelpers[field](this);
    }

    public delegate bool ValidationFunc(Data data, int field, out string msg);
    public abstract List<ValidationFunc>[] StaticValidationFuncs { get; }
    
    public void FieldValidationFuncsClear(int field)
    {
        if (StaticValidationFuncs[field] == null) return;
        StaticValidationFuncs[field].Clear();
    }
    public void FieldValidationFuncSet(int field, ValidationFunc func)
    {
        if (field < 0 || field >= FieldCount) return;
        if (StaticValidationFuncs[field]==null)
        {
            StaticValidationFuncs[field] = new List<ValidationFunc>();
        }
        StaticValidationFuncs[field].Clear();
        StaticValidationFuncs[field].Add(func);
    }
    public void FieldValidationFuncAdd(int field, ValidationFunc func)
    {
        if (field < 0 || field >= FieldCount) return;
        if (StaticValidationFuncs[field] == null)
        {
            StaticValidationFuncs[field] = new List<ValidationFunc>();
        }
        StaticValidationFuncs[field].Add(func);
    }
    public bool FieldHasValidation(int field)
    {
        if (field < 0 || field >= FieldCount) return false;
        if (StaticValidationFuncs[field] == null) return false;
        return StaticValidationFuncs[field].Count > 0;
    }
    public virtual bool FieldValidate(int field, out string msg)
    {
        msg = "";
        if (field < 0 || field >= FieldCount) return true;
        if (StaticValidationFuncs[field] == null) return true;
        foreach (var func in StaticValidationFuncs[field])
        {
            if (func(this,field, out msg) == false) return false;
        }
        return true;
    }
    public virtual void Dispose()
    {
        if (_editHelpers != null)
        {
            foreach (var eh in _editHelpers)
            {
                eh?.Dispose();
            }
            _editHelpers = null;
        }
    }
    public bool IsEqual(Data to)
    {
        if (GetType() != to.GetType()) return false;   
        if (to == null) return false;
        if (FieldCount != to.FieldCount) return false;
        for (int i = 0; i < FieldCount; i++)
        {
            if (FieldIsEqual(to, i) == false) return false;
        }
        return true;
    }
    public abstract bool IsRecord { get; }
    public Data()
    {
        OnBaseConstruct();
    }
    public virtual string LookupString
    {
        get
        {
            int nameField = 0;
            if (IsRecord) nameField = 4;
            for (int i = nameField; i < FieldCount; i++)
            {
                if (FieldType(i) == typeof(string))
                {
                    nameField = i;
                    break;
                }
            }
            return FieldAsString(nameField);
        }
    }
}
