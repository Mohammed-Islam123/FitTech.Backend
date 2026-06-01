package com.fittech.shop.model;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;
import org.springframework.data.annotation.Id;
import org.springframework.data.relational.core.mapping.Table;

import java.time.LocalDateTime;
import java.util.UUID;

@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
@Table("purchases")
public class Purchase {

    @Id
    private UUID id;

    private UUID memberId;

    private UUID productId;

    private Integer quantity;

    private LocalDateTime purchasedAt;
}
