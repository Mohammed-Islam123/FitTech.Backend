package com.fittech.equipments.messaging;

import com.fasterxml.jackson.annotation.JsonProperty;

/**
 * DTO matching the .NET SendEmailEvent record in Shared.Events namespace.
 * <pre>
 *   namespace Shared.Events;
 *   public record SendEmailEvent(string To, string Subject, string Body, bool IsHtml = true);
 * </pre>
 */
public class SendEmailEventDto {

    @JsonProperty("To")
    private String to;

    @JsonProperty("Subject")
    private String subject;

    @JsonProperty("Body")
    private String body;

    @JsonProperty("IsHtml")
    private boolean isHtml = true;

    public SendEmailEventDto() {
    }

    public SendEmailEventDto(String to, String subject, String body) {
        this.to = to;
        this.subject = subject;
        this.body = body;
    }

    public SendEmailEventDto(String to, String subject, String body, boolean isHtml) {
        this.to = to;
        this.subject = subject;
        this.body = body;
        this.isHtml = isHtml;
    }

    public String getTo() { return to; }
    public void setTo(String to) { this.to = to; }
    public String getSubject() { return subject; }
    public void setSubject(String subject) { this.subject = subject; }
    public String getBody() { return body; }
    public void setBody(String body) { this.body = body; }
    public boolean isHtml() { return isHtml; }
    public void setHtml(boolean html) { isHtml = html; }
}
