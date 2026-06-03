package com.fittech.equipments.messaging;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.amqp.rabbit.core.RabbitTemplate;
import org.springframework.stereotype.Service;

/**
 * Publishes MassTransit-compatible events to RabbitMQ fanout exchanges.
 */
@Service
public class MessagePublisher {

    private static final Logger log = LoggerFactory.getLogger(MessagePublisher.class);

    private static final String DOTNET_NAMESPACE = "Shared.Events";
    private static final String SEND_EMAIL_CLASS = "SendEmailEvent";

    private final RabbitTemplate rabbitTemplate;

    public MessagePublisher(RabbitTemplate rabbitTemplate) {
        this.rabbitTemplate = rabbitTemplate;
    }

    /**
     * Publishes a SendEmailEvent to the MassTransit fanout exchange.
     * The Notification service (.NET) consumes this and delivers the email.
     */
    public void publishSendEmail(String to, String subject, String body) {
        var dto = new SendEmailEventDto(to, subject, body, true);
        var envelope = new MassTransitEnvelope<>(dto, DOTNET_NAMESPACE, SEND_EMAIL_CLASS);
        var exchangeName = DOTNET_NAMESPACE + ":" + SEND_EMAIL_CLASS;
        rabbitTemplate.convertAndSend(exchangeName, "", envelope);
        log.info("Published SendEmailEvent to exchange {} for {}", exchangeName, to);
    }
}
