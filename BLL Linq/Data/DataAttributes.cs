using System;

namespace bigWebApps.HelpDesk.WebApi
{
    public class DataIgnoreAttrib : Attribute
    {
    }

    public class DataNameAttrib : Attribute
    {
        public string DataName { get; private set; }
        public DataNameAttrib(string DN)
        {
            DataName = DN;
        }
    }

    public class DataReadonlyAttrib : Attribute
    {
        public string DataName { get; private set; }
        public DataReadonlyAttrib(string DN)
        {
            DataName = DN;
        }
        public DataReadonlyAttrib()
        {
            DataName = null;
        }
    }

    public static class Utils
    {
        public static void CopyProperties<TSource>(TSource Source, object LinqDestination)
        {
            Type DestinationType = LinqDestination.GetType();
            Type SourceType = typeof(TSource);

            System.Reflection.FieldInfo[] SourceFields = SourceType.GetFields();
            foreach (System.Reflection.FieldInfo SourceField in SourceFields)
            {
                //bool DataIgnore = false;
                object[] attr = SourceField.GetCustomAttributes(typeof(DataIgnoreAttrib), true);
                if (attr.Length > 0) continue;//DataIgnore = true;

                string DestPropName = SourceField.Name;
                attr = SourceField.GetCustomAttributes(typeof(DataNameAttrib), true);
                if (attr.Length > 0) DestPropName = ((DataNameAttrib)attr[0]).DataName;

                System.Reflection.PropertyInfo DestProp = DestinationType.GetProperty(DestPropName);
                if (DestProp == null) continue;
                //if ((!DestProp.PropertyType.FullName.StartsWith("System.")) && DataIgnore) continue;
                object v = SourceField.GetValue(Source);
                if(v!=null) v = Convert.ChangeType(v, DestProp.PropertyType);
                DestProp.SetValue(LinqDestination, v, null);
            }
        }

        public static TDestination CopyProperties<TDestination>(object LinqSource)
        {
            Type SourceType = LinqSource.GetType();
            Type DestinationType = typeof(TDestination);
            System.Reflection.ConstructorInfo ci = DestinationType.GetConstructor(new Type[0]);
            TDestination rez = (TDestination)ci.Invoke(new object[0]);

            object ComplexSourceProp = null;
            Type ComplexSourcePropType = null;
            System.Reflection.PropertyInfo[] SourceProps = SourceType.GetProperties();
            foreach (System.Reflection.PropertyInfo SourceProp in SourceProps)
            {
                if (SourceProp.PropertyType.IsClass)
                {
                    ComplexSourceProp = SourceProp.GetValue(LinqSource, null);
                    ComplexSourcePropType = ComplexSourceProp.GetType();
                    break;
                }
            }

            System.Reflection.FieldInfo[] DestFields = typeof(TDestination).GetFields();
            foreach (System.Reflection.FieldInfo DestField in DestFields)
            {
                bool DataIgnore = false;
                object[] attr = DestField.GetCustomAttributes(typeof(DataIgnoreAttrib), true);
                if (attr.Length > 0) DataIgnore = true;

                string DestPropName = DestField.Name;
                string UpDestPropName = DestPropName.Substring(0, 1).ToUpper() + DestPropName.Substring(1);
                attr = DestField.GetCustomAttributes(typeof(DataNameAttrib), true);
                if (attr.Length > 0)
                {
                    DestPropName = ((DataNameAttrib)attr[0]).DataName;
                    UpDestPropName = DestPropName.Substring(0, 1).ToUpper() + DestPropName.Substring(1);
                }

                System.Reflection.PropertyInfo SourceProp = SourceType.GetProperty(DestPropName);
                if(SourceProp==null) SourceProp = SourceType.GetProperty(UpDestPropName);
                if (SourceProp == null && DataIgnore) continue;
                if (SourceProp == null)
                {
                    SourceProp = ComplexSourcePropType.GetProperty(DestPropName);
                    if (SourceProp == null) SourceProp = ComplexSourcePropType.GetProperty(UpDestPropName);
                    //**************************************************************************************************************************************************************************
                    //if (SourceProp == null) continue;//**************************************************************************************************************************************************************************
                    //if (SourceProp.PropertyType.IsClass) continue;
                    //**************************************************************************************************************************************************************************
                    object sv = SourceProp.GetValue(ComplexSourceProp, null);
                    DestField.SetValue(rez, sv);
                }
                else
                {
                    DestField.SetValue(rez, SourceProp.GetValue(LinqSource, null));
                }
            }
            return rez;
        }

        public static TDestination[] CopyProperties<TDestination>(System.Collections.IEnumerable Source)
        {
            System.Collections.ArrayList rez = new System.Collections.ArrayList();
            foreach (object s in Source)
            {
                TDestination des = CopyProperties<TDestination>(s);
                rez.Add(des);
            }
            TDestination[] ret = (TDestination[])rez.ToArray(typeof(TDestination));
            return ret;
        }
    }
}
