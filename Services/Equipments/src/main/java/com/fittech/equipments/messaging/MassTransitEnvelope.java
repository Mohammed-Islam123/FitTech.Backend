package com.fittech.equipments.messaging;

import java.util.UUID;

/**
 * Generic envelope that mirrors MassTransit's wire-format.
 * <p>
 * MassTransit expects a JSON object with metadata (messageId, conversationId, messageType)
 * wrapping the actual payload in a {@code message} property.
 *
 * @param <T> the inner payload type
 */
public class MassTransitEnvelope<T> {

    public String messageId = UUID.randomUUID().toString();
    public String conversationId = UUID.randomUUID().toString();
    public String[] messageType;
    public T message;

    public MassTransitEnvelope() {
    }

    public MassTransitEnvelope(T message, String dotnetNamespace, String dotnetClassName) {
        this.message = message;
        this.messageType = new String[]{
                "urn:message:" + dotnetNamespace + ":" + dotnetClassName
        };
    }

    public String getMessageId() { return messageId; }
    public void setMessageId(String messageId) { this.messageId = messageId; }
    public String getConversationId() { return conversationId; }
    public void setConversationId(String conversationId) { this.conversationId = conversationId; }
    public String[] getMessageType() { return messageType; }
    public void setMessageType(String[] messageType) { this.messageType = messageType; }
    public T getMessage() { return message; }
    public void setMessage(T message) { this.message = message; }
}
