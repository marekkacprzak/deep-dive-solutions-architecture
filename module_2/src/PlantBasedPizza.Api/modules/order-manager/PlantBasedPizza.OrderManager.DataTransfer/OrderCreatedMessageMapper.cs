// Unless explicitly stated otherwise all files in this repository are licensed under the Apache License Version 2.0.
// This product includes software developed at Datadog (https://www.datadoghq.com/).
// Copyright 2025 Datadog, Inc.

using System.Text.Json;
using Paramore.Brighter;

namespace PlantBasedPizza.OrderManager.DataTransfer;

public class OrderCreatedMessageMapper: IAmAMessageMapper<OrderCreatedEventV1> {
    public Message MapToMessage(OrderCreatedEventV1 request)
    {
        var header = new MessageHeader(messageId: request.Id, topic: request.EventName, messageType: MessageType.MT_EVENT);
        var body = new MessageBody(request.AsString());
        var message = new Message(header, body);
        return message;
    }

    public OrderCreatedEventV1 MapToRequest(Message message)
    {
        return JsonSerializer.Deserialize<OrderCreatedEventV1>(message.Body.Value);
    }
}