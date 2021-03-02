using System;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Basket.API.Entities;
using Basket.API.Repositories.Interfaces;
using EventBusRabbitMQ.Common;
using EventBusRabbitMQ.Events;
using EventBusRabbitMQ.Procuder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Basket.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class BasketController:ControllerBase
    {
        private readonly IBasketRepository _repository;
        private readonly IMapper _mapper;
        private readonly EventBusRabbitMQProducer _eventBus;

        public BasketController(IBasketRepository repository, IMapper mapper, EventBusRabbitMQProducer eventBus)
        {
            _repository = repository;
            _mapper = mapper;
            _eventBus = eventBus;
        }

        [HttpGet("userName")]
        [ProducesResponseType(typeof(BasketCart),(int)HttpStatusCode.OK)]
        public async Task<ActionResult<BasketCart>> Get(string userName)
        {
            var basket = await _repository.GetBasketAsync(userName);
            return Ok(basket??new BasketCart(userName));
        }

        [HttpPost]
        [ProducesResponseType(typeof(BasketCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<BasketCart>> UpdateBasket([FromBody] BasketCart basket)
        {
            var updatedBasket = await _repository.UpdateBasketAsync(basket);
            return Ok(updatedBasket);
        }

        [HttpDelete("userName")]
        [ProducesResponseType(typeof(BasketCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<BasketCart>> Delete(string userName)
        {
            var result = await _repository.DeleteBasketAsync(userName);
            return Ok(result);
        }
        [Route("[action]")]
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<BasketCart>> Checkout([FromBody] BasketCheckout basketCheckout)
        {
            var basket = await _repository.GetBasketAsync(basketCheckout.UserName);
            if (basket == null) return NotFound();

            var basketRemoved = await _repository.DeleteBasketAsync(basketCheckout.UserName);
            if (!basketRemoved) return NotFound();

            var eventMessage = _mapper.Map<BasketCheckoutEvent>(basketCheckout);
           
            try
            {
                _eventBus.PublishBasketCheckout(EventBusConstants.BasketCheckoutQueue,eventMessage);
            }
            catch (Exception e)
            {
                throw;
            }
            return Accepted();
        }
    }
}