package com.fittech.shop.controller;

import com.fittech.shop.dto.PurchaseResponse;
import com.fittech.shop.service.PurchaseService;
import org.springframework.http.HttpStatus;
import org.springframework.web.bind.annotation.*;
import reactor.core.publisher.Flux;
import reactor.core.publisher.Mono;

import java.util.Map;
import java.util.UUID;

@RestController
@RequestMapping("/api/purchases")
public class PurchaseController {

    private final PurchaseService purchaseService;

    public PurchaseController(PurchaseService purchaseService) {
        this.purchaseService = purchaseService;
    }

    @GetMapping
    public Flux<PurchaseResponse> listAll() {
        return purchaseService.listAll();
    }

    @PostMapping
    @ResponseStatus(HttpStatus.CREATED)
    public Mono<PurchaseResponse> process(@RequestBody Map<String, Object> body) {
        var productIdStr = (String) body.get("productId");
        var memberIdStr = (String) body.get("memberId");
        var quantityObj = body.get("quantity");

        if (productIdStr == null || productIdStr.isBlank()) {
            return Mono.error(new IllegalArgumentException("productId is required"));
        }

        var productId = UUID.fromString(productIdStr);
        var memberId = (memberIdStr != null && !memberIdStr.isBlank())
                ? UUID.fromString(memberIdStr)
                : null;
        var quantity = (quantityObj instanceof Number n) ? n.intValue() : 1;

        return purchaseService.process(productId, memberId, quantity);
    }
}
