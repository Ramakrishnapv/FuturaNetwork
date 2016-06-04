using ESRI.ArcGIS.Geodatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Futura.ArcGIS.NetworkExtension
{
    public static class DatabaseUtil
    {
        public static IFeature GetReadOnlyFeature(IWorkspace wrkspace, int objClassId, int OID)
        {
            IFeature feat = null;
            IFeatureWorkspaceManage3 wsManage3 = wrkspace as IFeatureWorkspaceManage3;

            string objClassName = wsManage3.GetObjectClassNameByID(objClassId);
            if (!string.IsNullOrEmpty(objClassName))
            {
                IFeatureWorkspace featWorkspace = wrkspace as IFeatureWorkspace;
                IFeatureClass featClass = featWorkspace.OpenFeatureClass(objClassName);
                if(featClass != null)
                {
                    feat = featClass.GetFeature(OID);
                    Marshal.ReleaseComObject(featClass);
                }
            }

            return feat;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wrkspace"></param>
        /// <param name="engPhaseByOIDByClsID"></param>
        /// <param name="failedFeatOIDsByClsID"></param>
        public static void UpdateEngPhase(IWorkspace wrkspace, Dictionary<int, Dictionary<int, int>> engPhaseByOIDByClsID, out Dictionary<int, int> failedFeatOIDsByClsID) //ExtensionInfo.Editor.EditWorkspace
        {
            ExtensionInfo.InternalEdit = true;
            ExtensionInfo.UpdateConnectivity = false;
            
            failedFeatOIDsByClsID = new Dictionary<int, int>();
            IFeatureCursor featCur = null;
            IFeature feat = null;
            try
            {
                IFeatureWorkspaceManage3 wsManage3 = wrkspace as IFeatureWorkspaceManage3;
                if (wsManage3 != null && engPhaseByOIDByClsID != null)
                {
                    foreach (KeyValuePair<int, Dictionary<int, int>> engPhWithOID in engPhaseByOIDByClsID)
                    {
                        string objClassName = wsManage3.GetObjectClassNameByID(engPhWithOID.Key);
                        if (!string.IsNullOrEmpty(objClassName))
                        {
                            IFeatureWorkspace featWorkspace = wrkspace as IFeatureWorkspace;
                            IFeatureClass featClass = featWorkspace.OpenFeatureClass(objClassName);
                            if (featClass != null)
                            {
                                int[] ary;
                                ary = engPhWithOID.Value.Keys.ToArray();
                                 featCur = featClass.GetFeatures(ary, false);
                                 feat = featCur.NextFeature();
                                 while (feat != null)
                                 {
                                     if (engPhWithOID.Value.ContainsKey(feat.OID))
                                     {
                                         DatabaseUtil.SetFieldValue(feat, ExtensionInfo.netUtil.ntInfo.phaseCodeField, engPhWithOID.Value[feat.OID]);
                                         feat.Store();
                                     }
                                     feat = featCur.NextFeature();
                                 }

                                 Marshal.ReleaseComObject(featCur);
                                Marshal.ReleaseComObject(featClass);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                ExtensionInfo.InternalEdit = false;
                ExtensionInfo.UpdateConnectivity = true;
            }

        }


        public static bool SetFieldValue(IRow row, string fieldName, object value)
        {
            bool success = false;
            if (row != null && string.IsNullOrEmpty(fieldName) == false)
            {
                int fieldIndex = GetFieldIndex(row.Fields, fieldName);
                if (fieldIndex != -1)
                {
                    success = SetFieldValue(row, fieldIndex, value);
                }
               
            }
           return success;
        }

        private static object ToFieldType(IField field, object value)
        {
            if (field != null)
            {
                try
                {
                    esriFieldType fieldType = field.Type;
                    if (field.IsNullable & value == null)
                        return null;
                    switch (fieldType)
                    {
                        case esriFieldType.esriFieldTypeDate:
                            return ToDateTime(value);
                        case esriFieldType.esriFieldTypeString:
                            return ToText(value);
                        case esriFieldType.esriFieldTypeInteger:
                            return ToInteger(value, -1);
                        case esriFieldType.esriFieldTypeDouble:
                            return ToDouble(value);
                        case esriFieldType.esriFieldTypeSmallInteger:
                            return ToInteger(value, -1);
                        default:
                            return value;
                    }
                }
                catch (Exception e)
                {
                    //_logger.LogException(e);
                    //_logger.LogFormat("{0}: Failed converting {1} in to field [{2}].", methodName, Converter.ToText(value), field.Name, LogLevel.enumLogLevelWarn);
                    return null;
                }
            }
            else
            {
                //_logger.LogFormat("{0}: Null field parameter.", methodName, LogLevel.enumLogLevelWarn);
                return null;
            }
        }

        public static object CodeFromValue(IDomain domain, object value)
        {
            object code = null;

            if (domain != null)
            {
                if (domain is ICodedValueDomain)
                {
                    ICodedValueDomain codedValueDomain = domain as ICodedValueDomain;
                    string convertedValue = ToText(value);

                    for (int i = 0; i < codedValueDomain.CodeCount; i++)
                    {
                        if (AreStringsEqual(convertedValue, codedValueDomain.get_Name(i)))
                            code = codedValueDomain.get_Value(i);
                    }
                }
            }

            return code;
        }

        public static bool SetFieldValue(IRow row, int fieldIndex, object value)
        {
            bool success = false;
            if (fieldIndex != -1)
            {
                IField field = row.Fields.get_Field(fieldIndex);
                string fieldName = field.Name;
                if (field.Editable == true)
                {
                    IDomain domain = GetFieldDomain(field, row);
                    if (domain == null)
                        value = ToFieldType(field, value);
                    else
                    {
                        bool isValid = false;
                        object o = CodeFromValue(domain, value);

                        if (o != null)
                        {
                            if (!string.IsNullOrEmpty(ToText(o)))
                            {
                                isValid = true;
                                value = ToText(o);
                            }
                        }

                        if (!isValid)
                            isValid = ValueFromCode(domain, value) != string.Empty;
                       
                    }
                    try
                    {
                        row.set_Value(fieldIndex, value);
                        success = true;
                    }
                    catch (Exception e)
                    {
                       // _logger.LogException(e);
                    }
                }
            }
            return success;
        }

        public static IField GetSubTypeField(IRow row)
        {
            return GetSubtypeField(row.Table);
        }

        public static IField GetSubtypeField(IClass cls)
        {

            IField field = null;
            if (cls != null)
            {
                ISubtypes subTypes = cls as ISubtypes;
                if (subTypes != null)
                {
                    if (subTypes.HasSubtype == true)
                    {
                        int subtypeFieldIndex = subTypes.SubtypeFieldIndex;
                        try
                        {
                            field = cls.Fields.get_Field(subtypeFieldIndex);
                        }
                        catch (Exception e)
                        {
                            //_logger.LogException(e, "Error getting subtype field with fieldindex = " + subtypeFieldIndex + " for: " + GetQualifiedName(cls));
                        }
                    }
                }
            }
            //else
            //    _logger.LogFormat("{0}: Null class parameter.", methodName, LogLevel.enumLogLevelWarn);


            return field;
        }

        public static string GetSubTypeFieldName(IClass cls)
        {

            string subTypeFieldName = string.Empty;
            if (cls != null)
            {
                IField f = GetSubtypeField(cls);
                if (f != null)
                    subTypeFieldName = f.Name;
            }

            return subTypeFieldName;
        }

        public static object GetSubTypeValue(IRow row)
        {
            return GetFieldValue(row, GetSubTypeFieldName(row.Table));
        }

        public static void SetDefaultValueForField(IRow row, IField fld)
        {
            if (row != null && fld != null)
            {
                object o = GetDefaultValueForField(row, fld);
                SetFieldValue(row, fld.Name, o);
            }
        }

        public static bool DoesDomainMemberExist(ICodedValueDomain cvDomain, object code)
        {
            return !string.IsNullOrEmpty(ValueFromCode(cvDomain as IDomain, code));
        }

        public static object GetDefaultValueForField(IRow row, IField fld)
        {
            object defaultValue = DBNull.Value;
            if (!fld.IsNullable)
                defaultValue = string.Empty;

            if (row != null && fld != null)
            {
                IClass cls = row.Table;
                if (HasSubtype(cls))
                {
                    ISubtypes subTypes = (ISubtypes)cls;
                    string subTypeFieldName = GetSubTypeFieldName(cls);
                    if (string.IsNullOrEmpty(subTypeFieldName) == false)
                    {
                        object o = GetFieldValue(row, subTypeFieldName);
                        int subTypeCode = ToInteger(o, -1);
                        if (subTypeCode != -1)
                            defaultValue = subTypes.get_DefaultValue(subTypeCode, fld.Name);
                    }
                }

                if (defaultValue == DBNull.Value && fld.Type == esriFieldType.esriFieldTypeDate)
                    defaultValue = fld.DefaultValue;
            }
            return defaultValue;
        }

        /// <summary>
        /// Sets the defaults field values for all the fields in the given row.
        /// </summary>
        /// <param name="row">The row for which to set default field values.</param>
        /// <param name="doEmptyValuesOnly">Boolean determining whether to process fields with empty values only.</param>
        public static void SetDefaultValues(IRow row, bool doEmptyValuesOnly)
        {
            if (row != null)
            {
                if (HasSubtype(row.Table))
                {
                    IRowSubtypes rowSubtype = row as IRowSubtypes;
                    if (rowSubtype != null)
                    {

                        int defaultCode = ((ISubtypes)row.Table).DefaultSubtypeCode;
                        int currentCode = ToInteger(GetSubTypeValue(row), -1);
                        if (currentCode >= 0)
                            rowSubtype.SubtypeCode = currentCode;
                        else
                            rowSubtype.SubtypeCode = defaultCode;
                        rowSubtype.InitDefaultValues();
                    }
                }
                else
                {
                    object o;
                    string s;
                    for (int i = 0; i < row.Fields.FieldCount; i++)
                    {
                        o = row.get_Value(i);
                        s = ToText(o);
                        if (doEmptyValuesOnly)
                        {
                            if (string.IsNullOrEmpty(s))
                            {
                                if (o == DBNull.Value && row.Fields.get_Field(i).IsNullable || o != DBNull.Value)
                                    row.set_Value(i, row.Fields.get_Field(i).DefaultValue);
                            }
                        }
                        else
                        {
                            object defaultValue = row.Fields.get_Field(i).DefaultValue;
                            if (defaultValue != string.Empty && defaultValue != null && defaultValue != DBNull.Value)
                                row.set_Value(i, row.Fields.get_Field(i).DefaultValue);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets the defaults field values for all the fields in the given row.
        /// </summary>
        /// <param name="row">The row for which to set default field values.</param>
        public static void SetDefaultValues(IRow row)
        {
            SetDefaultValues(row, true);
        }

        public static object GetFieldValue(IRow row, int fieldIndex, bool getDomainDescription)
        {
            object o = null;
            if (row != null && fieldIndex >= 0)
            {
                try
                {
                    IField field = row.Fields.get_Field(fieldIndex);
                    if (field != null)
                    {
                        o = row.get_Value(fieldIndex);
                        if (getDomainDescription)
                        {
                            IDomain domain = GetFieldDomain(field, row);
                            if (domain != null)
                                o = ValueFromCode(domain, o);
                            else if (field.Domain != null)
                                o = ValueFromCode(field.Domain, o);
                        }
                    }
                }
                catch (Exception e)
                {
                    //_logger.LogException(e);
                }
            }
            return o;
        }

        public static object GetDomainDescription(IRow row, int fieldIndex, object value)
        {
            object o = value;
            if(row != null && fieldIndex != -1 && value != null)
            {
                 IField field = row.Fields.get_Field(fieldIndex);
                 if (field != null)
                 {
                     IDomain domain = GetFieldDomain(field, row);
                     if (domain != null)
                         o = ValueFromCode(domain, o);
                     else if (field.Domain != null)
                         o = ValueFromCode(field.Domain, o);
                 }
            }
            return o;
        }
        /// <summary>
        /// Gets the domain for a field if it exists.
        /// </summary>
        /// <param name="field">The field for which to get the domain.</param>
        /// <param name="row">A row.</param>
        /// <returns>Returns the domain for the given field if one exists, otherwise returns null.</returns>
        public static IDomain GetFieldDomain(IField field, IRow row)
        {
          //  string methodName = MethodInfo.GetCurrentMethod().Name;
            IDomain domain = null;
            if (field != null && row != null)
            {
                IClass rowClass = row.Table as IClass;
                string fieldName = field.Name.ToLower();

                if (rowClass != null)
                {
                    if (HasSubtype(rowClass) == true)
                    {
                        ISubtypes subtypes = rowClass as ISubtypes;
                        if (subtypes != null)
                        {
                            string subtypeFieldName = subtypes.SubtypeFieldName.ToLower();
                            int subtypeCode = ToInteger(GetFieldValue(row, subtypeFieldName), -1);

                            //load the different subtypes in to a domain
                            if (fieldName == subtypeFieldName)
                                domain = CreateCodedValueDomainFromSubtypes(subtypes) as IDomain;
                            //otherwise try to get the field domain based off the subtype code 
                            else
                                domain = subtypes.get_Domain(subtypeCode, fieldName);

                            if (domain == null)
                                domain = field.Domain;
                        }
                    }
                    //No subtypes try to get field domain.
                    else
                        domain = field.Domain;
                }
                //else
                //    _logger.LogFormat("{0}: Could not QI the row.Table.", methodName, LogLevel.enumLogLevelWarn);
            }
            //else
            //    _logger.LogFormat("{0}: Null field or row parameter.", methodName, LogLevel.enumLogLevelWarn);

            return domain;
        }

        public static bool HasSubtype(IClass myClass)
        {
           // string methodName = MethodInfo.GetCurrentMethod().Name;

            bool hasSubtype = false;
            if (myClass != null)
            {
                ISubtypes subTypes = myClass as ISubtypes;
                if (subTypes != null)
                    hasSubtype = subTypes.HasSubtype;
                else
                    hasSubtype = false;
            }
            //else
            //    _logger.LogFormat("{0}: Null class parameter.", methodName, LogLevel.enumLogLevelWarn);

            return hasSubtype;
        }

        public static ICodedValueDomain CreateCodedValueDomainFromSubtypes(ISubtypes subtypes)
        {
          //  string methodName = MethodInfo.GetCurrentMethod().Name;
            ICodedValueDomain domain = null;
            if (subtypes != null)
            {
                IEnumSubtype subtype = subtypes.Subtypes;
                domain = new CodedValueDomainClass();

                subtype.Reset();
                int subtypeCode;
                string subtypeValue = subtype.Next(out subtypeCode);
                while (subtypeValue != null) //use null subtype.next returns bstr type.
                {
                    domain.AddCode(subtypeCode, subtypeValue);
                    subtypeValue = subtype.Next(out subtypeCode);
                }
            }
            //else
            //    _logger.LogFormat("{0}: Null subtypes parameter.", methodName, LogLevel.enumLogLevelWarn);

            return domain;
        }
        /// <summary>
        /// Gets the field value from the given row for the given field.
        /// </summary>
        /// <param name="row">The row from which to get the value.</param>
        /// <param name="fieldName">The name of the field for which to get the value.</param>
        /// <returns>Returns an object representing the value of the given field name.</returns>
        public static object GetFieldValue(IRow row, string fieldName)
        {
            return GetFieldValue(row, fieldName, false);
        }
        /// <summary>
        /// Gets the field value from the given row for the given field.
        /// </summary>
        /// <param name="row">The row from which to get the value.</param>
        /// <param name="fieldName">The name of the field for which to get the value.</param>
        /// <param name="getDomainDescription">Boolean determining whether to get the domain description instead of the domain code.</param>
        /// <returns>Returns an object representing the value of the given field name.</returns>
        public static object GetFieldValue(IRow row, string fieldName, bool getDomainDescription)
        {
            object o = null;
            if (row != null && string.IsNullOrEmpty(fieldName) == false)
            {
                IField field = GetField(row, fieldName);
                if (field != null)
                {
                    o = GetRawFieldValue(row, fieldName);
                    if (getDomainDescription)
                    {
                        IDomain domain = GetFieldDomain(field, row);
                        if (domain != null)
                            o = ValueFromCode(domain, o);
                    }
                }
            }
            return o;
        }

        public static object GetRawFieldValue(IRow row, string fieldName)
        {
            object returnValue = null;
            if (row != null && string.IsNullOrEmpty(fieldName) == false)
            {
                int fieldIndex = GetFieldIndex(row.Fields, fieldName);
                if (fieldIndex != -1)
                {
                    try
                    {
                        returnValue = row.get_Value(fieldIndex);
                    }
                    catch (Exception e)
                    {
                        //_logger.LogException(e);
                    }
                }
                //else
                //    _logger.LogFormat("{0}: {1} field does not exist.", methodName, fieldName, LogLevel.enumLogLevelWarn);
            }
            //else
            //    _logger.LogFormat("{0}: Null row or field name parameter.", methodName, LogLevel.enumLogLevelWarn);

            return returnValue;
        }
        /// <summary>
        /// Gets a field given a row and field name.
        /// </summary>
        /// <param name="row">A row from which to get the fields collection.</param>
        /// <param name="fieldName">The name of the field to get.</param>
        /// <returns>Returns the field.</returns>
        public static IField GetField(IRow row, string fieldName)
        {
           // string methodName = MethodInfo.GetCurrentMethod().Name;
            IField field = null;
            if (row != null)
            {
                IClass myClass = row.Table as IClass;
                field = GetField(myClass, fieldName);
            }
            //else
              //  _logger.LogFormat("{0}: Null row parameter.", methodName, LogLevel.enumLogLevelWarn);

            return field;
        }

        /// </summary>
        /// <param name="myClass">The class for which to get the field.</param>
        /// <param name="fieldName">The name of the field to get.</param>
        /// <returns>Returns the field for the given field name.</returns>
        public static IField GetField(IClass myClass, string fieldName)
        {
           // string methodName = MethodInfo.GetCurrentMethod().Name;

            IField field = null;
            if (myClass != null && fieldName != string.Empty)
            {
                int i = GetFieldIndex(myClass.Fields, fieldName);
                if (i != -1)
                    field = myClass.Fields.get_Field(i);
            }
            //else
            //    _logger.LogFormat("{0}: Null class or field name parameter.", methodName, LogLevel.enumLogLevelWarn);

            return field;
        }

        /// <summary>
        /// Get the field index of the specified field in the fields collection.
        /// </summary>
        /// <param name="fields">The fields collection.</param>
        /// <param name="fieldName">The name of the field for which to get the field index.</param>
        /// <returns>Returns the field index of the given field name if it exists, -1 if it does not.</returns>
        public static int GetFieldIndex(IFields fields, string fieldName)
        {
            //string methodName = MethodInfo.GetCurrentMethod().Name;

            int index = -1;
            if (fields != null && fieldName != String.Empty)
                index = fields.FindField(fieldName);
            //else
            //    _logger.LogFormat("{0}: Null fields collection or field name parameter.", methodName, LogLevel.enumLogLevelWarn);


            return index;
        }
        /// <summary>
        /// Get the field by index from the class.
        /// </summary>
        /// <param name="myClass">The class for which to get the field.</param>
        /// <param name="fieldName">The name of the field to get.</param>
        /// <returns>Returns the field for the given field name.</returns>
        public static IField GetField(IClass myClass, int fieldIndex)
        {
           // string methodName = MethodInfo.GetCurrentMethod().Name;

            IField field = null;
            if (myClass != null && fieldIndex != -1)
                field = myClass.Fields.get_Field(fieldIndex);
            //else
              //  _logger.LogFormat("{0}: Null class or field name parameter.", methodName, LogLevel.enumLogLevelWarn);

            return field;
        }
       
        /// <summary>
        /// Gets the domain description for a domain code.
        /// </summary>
        /// <param name="domain">The domain from which to get the description.</param>
        /// <param name="code">The code for which to get the description.</param>
        /// <returns>Returns the description for the given code.</returns>
        public static string ValueFromCode(IDomain domain, object code)
        {
            //string methodName = MethodInfo.GetCurrentMethod().Name;
            string value = string.Empty;

            if (domain == null)
                return value;

            if (domain is ICodedValueDomain)
            {
                ICodedValueDomain codedValueDomain = domain as ICodedValueDomain;
                string convertedValue = ToText(code);

                for (int i = 0; i < codedValueDomain.CodeCount; i++)
                {
                    if (AreStringsEqual(convertedValue, ToText(codedValueDomain.get_Value(i))))
                        value = codedValueDomain.get_Name(i);
                }
            }
           // else
              //  _logger.Log(String.Format("{0} :Domain does not implement ICodedValueDomain.", methodName), LogLevel.enumLogLevelWarn);

            return value;
        }

        public static bool AreStringsEqual(string s1, string s2)
        {
            if (string.Compare(s1, s2, true) == 0)
                return true;
            else
                return false;
        }


        public static string ToText(object value)
        {
           // string methodName = MethodInfo.GetCurrentMethod().Name;
            string ret = string.Empty;

            if (value != DBNull.Value && value != null && value != "")
            {
                try
                {
                    ret = Convert.ToString(value);
                    ret = ret.Trim();
                }
                catch (Exception e)
                {
                    //_logger.LogFormat("{0}: [{1}] {2}", methodName, e.TargetSite, e.Message, LogLevel.enumLogLevelDebug);
                }
            }
            //else
            //    _logger.LogFormat("{0}: Null value parameter.", methodName, LogLevel.enumLogLevelDebug);

            return ret;
        }

        public static bool ToBool(object value)
        {
            //string methodName = MethodInfo.GetCurrentMethod().Name;

            bool b = false;
            Int32 outValue;
            if (value != DBNull.Value && value != null && value != "")
            {
                if (Int32.TryParse(value.ToString(), out outValue))
                    value = Convert.ToInt32(value);
                try
                {
                    b = Convert.ToBoolean(value);
                }
                catch (Exception e)
                {
                   // _logger.LogFormat("{0}: [{1}] {2}", methodName, e.TargetSite, e.Message, LogLevel.enumLogLevelDebug);
                }
            }
            //else
            //    _logger.LogFormat("{0}: Null value parameter.", methodName, LogLevel.enumLogLevelDebug);

            return b;
        }

        public static double ToDouble(object value)
        {
            return ToDouble(value, -1);
        }

        public static double ToDouble(object value, int decimalPlaces)
        {
            double i = 0.0;
            //string methodName = MethodInfo.GetCurrentMethod().Name;

            //_logger.LogFormat("{0}: {1} decimal places.", methodName, decimalPlaces.ToString(), LogLevel.enumLogLevelDebug);

            if (value == DBNull.Value || value == null)
            {
              //  _logger.LogFormat("{0}: Null value parameter.", methodName, LogLevel.enumLogLevelDebug);
                return 0.0;
            }
            else
            {
                try
                {
                    i = Convert.ToDouble(value);
                    if (decimalPlaces >= 0)
                        i = Math.Round(i, decimalPlaces);
                }
                catch (Exception e)
                {
                   // _logger.LogFormat("{0}: [{1}] {2}", methodName, e.TargetSite, e.Message, LogLevel.enumLogLevelDebug);
                }
            }

            return i;
        }

        public static int ToInteger(object value, int errorValue)
        {
            int i = errorValue;
            //string methodName = MethodInfo.GetCurrentMethod().Name;

            if (value != DBNull.Value && value != null && value != "")
            {
                try
                {
                    i = Convert.ToInt32(value);
                }
                catch (Exception e)
                {
                    //_logger.LogFormat("{0}: [{1}] {2}", methodName, e.TargetSite, e.Message, LogLevel.enumLogLevelDebug);
                }
            }
            ////else
                //_logger.LogFormat("{0}: Null value parameter.", methodName, LogLevel.enumLogLevelDebug);

            return i;
        }

        public static DateTime ToDateTime(object value)
        {
            DateTime returnValue = new DateTime(1900, 1, 1);

            if (!Convert.IsDBNull(value) && value != null)
            {
                try
                {
                    returnValue = Convert.ToDateTime(value);
                }
                catch (Exception e)
                {
                    //_logger.LogFormat("{0}: [{1}] {2}", methodName, e.TargetSite, e.Message, LogLevel.enumLogLevelDebug);
                }
            }
            //else
                //_logger.LogFormat("{0}: Null value parameter.", methodName, LogLevel.enumLogLevelDebug);

            return returnValue;
        }

    }
}
