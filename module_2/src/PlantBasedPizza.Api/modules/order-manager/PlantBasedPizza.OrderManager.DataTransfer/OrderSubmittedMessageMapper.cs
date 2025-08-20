// Unless explicitly stated otherwise all files in this repository are licensed under the Apache License Version 2.0.
// This product includes software developed at Datadog (https://www.datadoghq.com/).
// Copyright 2025 Datadog, Inc.

using System.Text.Json;
using Paramore.Brighter;

namespace PlantBasedPizza.OrderManager.DataTransfer;

public class OrderSubmittedMessageMapper : IAmAMessageMapper<OrderSubmittedEventV1>
{
    public Message MapToMessage(OrderSubmittedEventV1 request)
    {
        var header = new MessageHeader(request.Id, request.EventName, MessageType.MT_EVENT);
        var body = new MessageBody(request.AsString());
        var message = new Message(header, body);
        return message;
    }

    public OrderSubmittedEventV1 MapToRequest(Message message)
    {
        var evt = JsonSerializer.Deserialize<OrderSubmittedEventV1>(message.Body.Value);

        return evt;
    }
}