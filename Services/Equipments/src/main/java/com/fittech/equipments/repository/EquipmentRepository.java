package com.fittech.equipments.repository;

import com.fittech.equipments.model.Equipment;
import org.springframework.data.repository.reactive.ReactiveCrudRepository;

import java.util.UUID;

public interface EquipmentRepository extends ReactiveCrudRepository<Equipment, UUID> {
}
