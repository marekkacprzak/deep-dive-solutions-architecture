// Unless explicitly stated otherwise all files in this repository are licensed under the Apache License Version 2.0.
// This product includes software developed at Datadog (https://www.datadoghq.com/).
// Copyright 2025 Datadog, Inc.

using System.Text.Json;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.OrderManager.DataTransfer;

public class OrderCreatedEventV1 : PublicEvent
{
    public static string EventTypeName => "orders.order-created";
    private readonly string _eventId;

    public OrderCreatedEventV1(string orderIdentifier)
    {
        this._eventId = Guid.NewGuid().ToString();
        this.EventDate = DateTime.Now.ToUniversalTime();
        this.OrderIdentifier = orderIdentifier;
    }

    public string OrderIdentifier { get; private set; }

    public override string EventName => "orders.order-created";

    public override string EventVersion => "v1";

    public override string EventId => this._eventId;

    public override DateTime EventDate { get; }

    public override string AsString()
    {
        return JsonSerializer.Serialize(this);
    }
}