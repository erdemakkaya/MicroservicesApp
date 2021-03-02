using System;
using AutoMapper;
using Basket.API.Entities;
using EventBusRabbitMQ.Events;

namespace Basket.API.Mapping
{
    public class MappingProfile:Profile
    {
        public MappingProfile()
        {
            CreateMap<BasketCheckout, BasketCheckoutEvent>()
                .ForMember(dest => dest.RequestId, opts => opts.MapFrom(src => Guid.NewGuid())).ReverseMap();
        }
    }
}
