using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using Ordering.Application.Responses;

namespace Ordering.Application.Commands
{
   public class CheckoutOrderCommand :IRequest<OrderResponse>
    {
    }
}
