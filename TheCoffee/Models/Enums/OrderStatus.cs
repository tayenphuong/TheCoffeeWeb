using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TheCoffee.Models.Enums
{
    public enum OrderStatus
    {
        Moi = 1,         // chờ xác nhận
        DangXuLy = 2,    // Đã xác nhận
        DaGiao = 3,      // đang giao
        DaHuy = 4,        // Đã hủy
        DaHoanThanh = 5 // Đã hoàn thành
    }
}