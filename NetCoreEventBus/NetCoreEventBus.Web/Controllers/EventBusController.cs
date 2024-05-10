﻿using Microsoft.AspNetCore.Mvc;
using NetCoreEventBus.Infra.EventBus.Bus;
using NetCoreEventBus.Web.Dtos;
using NetCoreEventBus.Web.IntegrationEvents.Events;

namespace NetCoreEventBus.Web.Controllers;

[ApiController]
[Route("api/event-bus")]
[Produces("application/json")]
public class EventBusController : Controller
{
    private readonly IEventBus _eventBus;

    public EventBusController(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    /// <summary>
    /// Sends a message through the event bus. This route is here for testing purposes.
    /// </summary>
    /// <param name="input">Message to send.</param>
    /// <returns>Message sent confirmation.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(string), 200)]
    public IActionResult SendMessage([FromBody] TestDto input)
    {
        input ??= new TestDto() { TestMessage = "" };
        _eventBus.Publish(new MessageSentEvent { Message = input.TestMessage });
        return Ok("Message sent.");
    }
}