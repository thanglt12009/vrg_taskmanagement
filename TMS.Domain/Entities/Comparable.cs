using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Domain.Entities
{
    public enum DisplayTypeE
    {
        Text = 1,
        Date,
        UserId,
        WorkflowState,
        BoardId,
        RelatedTask,
        CategoryTaskType,
        CategoryPriority,
        CategoryCompany,
    }
    public enum MetaValueTypeE
    {
        None,
        SingleValue,
        MultipleValue
    }
    public class DiffInfo
    {
        public string Title { get; set; }
        public string PropertyName { get; set; }
        public dynamic NewValue { get; set; }
        public dynamic OldValue { get; set; }
        public DisplayTypeE DisplayType { get; set; }
        public MetaValueTypeE MetaInfo { get; set; }
    }

    public class ComparableAttribute : System.Attribute
    {
        public DisplayTypeE DisplayType { get; set; }
        public MetaValueTypeE MetaInfo { get; set; }
    }

    public abstract class Comparable
    {
        public List<DiffInfo> Compare(Comparable oldObj)
        {
            Comparable newObj = this;
            List<DiffInfo> res = new List<DiffInfo>();

            Type nt = newObj.GetType();
            List<PropertyInfo> props = nt.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ComparableAttribute))).ToList();
            DisplayTypeE displayType;
            MetaValueTypeE metaInfo;
            string filedTitle;

            foreach (var p in props)
            {
                displayType = p.GetCustomAttribute<ComparableAttribute>().DisplayType;
                metaInfo = p.GetCustomAttribute<ComparableAttribute>().MetaInfo;
                filedTitle = string.Empty;
                DisplayAttribute d = p.GetCustomAttribute<DisplayAttribute>();

                var oldvalue = (oldObj == null ? string.Empty : p.GetValue(oldObj, null));
                var newvalue = (newObj == null ? string.Empty : p.GetValue(newObj, null));

                switch (metaInfo)
                {
                    case MetaValueTypeE.MultipleValue:
                        // temporary not count relatedtasks into history
                        break;
                    case MetaValueTypeE.SingleValue:
                    case MetaValueTypeE.None:
                        if (!Equals(oldvalue, newvalue))
                        {
                            res.Add(new DiffInfo
                            {
                                Title = (d == null ? p.Name : d.Name),
                                PropertyName = p.Name,
                                OldValue = oldvalue,
                                NewValue = newvalue,
                                DisplayType = displayType,
                                MetaInfo = metaInfo
                            });
                        }
                        break;
                }
                if (oldObj == null && res.Count > 0) break;
            }
            return res;
        }
    }
}
