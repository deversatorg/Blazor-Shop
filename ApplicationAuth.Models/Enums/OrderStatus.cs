﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationAuth.Models.Enums
{
    public enum OrderStatus
    {
        Created,
        SuccessfulPayment,
        PaymentError,
        ReadyToPick,
        OnTheWay,
    }
}
