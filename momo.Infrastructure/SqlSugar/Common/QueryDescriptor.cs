﻿using System;
using System.Collections.Generic;
using System.Text;

namespace momo.Infrastructure.Common
{
    /// <summary>
    /// 查询集合
    /// </summary>
    public class QueryDescriptor
    {
        /// <summary>
        /// 行数
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// 页码
        /// </summary>
        public int PageIndex { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public List<OrderByClause> OrderBys { get; set; }
        /// <summary>
        /// 条件
        /// </summary>
        public List<QueryCondition> Conditions { get; set; }
    }
}
