// Unless explicitly stated otherwise all files in this repository are licensed under the Apache License Version 2.0.
// This product includes software developed at Datadog (https://www.datadoghq.com/).
// Copyright 2025 Datadog, Inc.

using PlantBasedPizza.OrderManager.DataTransfer;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public interface OrderEventPublisher
{
    Task Publish(OrderCreatedEventV1 evt);
    Task Publish(OrderSubmittedEventV1 evt);
    Task Publish(OrderCompletedEventV1 evt);
}