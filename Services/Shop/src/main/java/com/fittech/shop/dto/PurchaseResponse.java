package com.fittech.shop.dto;

import com.fittech.shop.model.Purchase;
import java.time.LocalDateTime;
import java.util.UUID;

public record PurchaseResponse(
        UUID id,
        UUID memberId,
        UUID productId,
        Integer quantity,
        LocalDateTime purchasedAt
) {
    public static PurchaseResponse from(Purchase p) {
        return new PurchaseResponse(
                p.getId(),
                p.getMemberId(),
                p.getProductId(),
                p.getQuantity(),
                p.getPurchasedAt()
        );
    }
}
