using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace TMS.Domain.Common
{
    public static class Contain
    {
        public enum CatType
        {
            CategoryType = 0,
            Category = 1,
            Status = 2,
            Priority = 3,
            UserRole = 4,
            MetaType = 5,
            AttachmentType = 6,
            Company = 7
        }

        public enum StateType
        {
            Init = 0,
            Normal = 1,
            End = 2
        }

        public static string GetDisplayName(this Enum enumValue)
        {
            return enumValue.GetType().GetMember(enumValue.ToString())
                           .First()
                           .GetCustomAttribute<DisplayAttribute>()
                           .Name;
        }
    }
}
