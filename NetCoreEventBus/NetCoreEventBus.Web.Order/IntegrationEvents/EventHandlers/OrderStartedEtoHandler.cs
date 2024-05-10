﻿using NetCoreEventBus.Infra.EventBus.Events;
using NetCoreEventBus.Shared.Events;
using NetCoreEventBus.Web.Order.Services;

namespace NetCoreEventBus.Web.Order.IntegrationEvents.EventHandlers;

public class OrderStartedEtoHandler : IEventHandler<OrderStartedEto>
{
    private readonly ILogger<OrderStartedEtoHandler> _logger;
    private readonly IOrderService _orderService;

    public OrderStartedEtoHandler(ILogger<OrderStartedEtoHandler> logger, IOrderService orderService)
    {
        _logger = logger;
        _orderService = orderService;
    }

    public async Task HandleAsync(OrderStartedEto @event)
    {
        _logger.LogInformation("OrderStarted BEGIN => OrderId{OrderId}", @event.OrderId.ToString());
        await _orderService.OrderStartedAsync(@event);
    }
}