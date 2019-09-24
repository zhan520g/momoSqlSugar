using System;
using System.Collections.Generic;
using System.Text;

namespace momo.Infrastructure.Common
{
    /// <summary>
    /// 查询
    /// </summary>
    public class QueryCondition
    {
        /// <summary>
        /// 字段名称
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 查询操作
        /// </summary>
        public QueryOperator Operator { get; set; }
        /// <summary>
        /// 值
        /// </summary>
        public object Value { get; set; }

    }
}
