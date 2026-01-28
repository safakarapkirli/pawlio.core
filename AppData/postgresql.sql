-- =========================================================
-- 1. FUNCTION: UpdateBalance
-- =========================================================
DROP FUNCTION IF EXISTS UpdateBalance;

CREATE OR REPLACE FUNCTION UpdateBalance(
    pFirmId INT,
    pBranchId INT,
    pCustomerId INT,
    pSupplierId INT,
    pAmount NUMERIC(18,2)
)
RETURNS void
LANGUAGE plpgsql
AS $$
DECLARE
    vIsCustomer BOOLEAN := FALSE;
    vIsSupplier BOOLEAN := FALSE;
    v_rowcount  INT;
BEGIN
    IF pCustomerId IS NOT NULL THEN
        vIsCustomer := TRUE;
    ELSIF pSupplierId IS NOT NULL THEN
        vIsSupplier := TRUE;
    ELSE
        RETURN;
    END IF;

    IF vIsCustomer THEN
        UPDATE "Balances"
        SET "Balance" = "Balance" + pAmount,
            "Updated" = now()
        WHERE "FirmId" = pFirmId
          AND "CustomerId" = pCustomerId;

        GET DIAGNOSTICS v_rowcount = ROW_COUNT;

        IF v_rowcount = 0 THEN
            INSERT INTO "Balances"
                ("Created", "FirmId", "BranchId", "CustomerId", "SupplierId", "Balance")
            VALUES
                (now(), pFirmId, pBranchId, pCustomerId, NULL, pAmount);
        END IF;

    ELSIF vIsSupplier THEN
        UPDATE "Balances"
        SET "Balance" = "Balance" + pAmount,
            "Updated" = now()
        WHERE "FirmId" = pFirmId
          AND "SupplierId" = pSupplierId;

        GET DIAGNOSTICS v_rowcount = ROW_COUNT;

        IF v_rowcount = 0 THEN
            INSERT INTO "Balances"
                ("Created", "FirmId", "BranchId", "CustomerId", "SupplierId", "Balance")
            VALUES
                (now(), pFirmId, pBranchId, NULL, pSupplierId, pAmount);
        END IF;
    END IF;
END;
$$;


-- =========================================================
-- 2. FUNCTION: UpdateProductAmount
-- =========================================================
DROP FUNCTION IF EXISTS UpdateProductAmount;

CREATE OR REPLACE FUNCTION UpdateProductAmount(
    pType INT,
    pBranchId INT,
    pProductId INT,
    pQuantity NUMERIC(18,2)
)
RETURNS void
LANGUAGE plpgsql
AS $$
DECLARE
    vAmountChange NUMERIC(18,2);
    v_rowcount    INT;
BEGIN
    IF pType = 1 THEN
        vAmountChange := pQuantity;
    ELSIF pType = 2 THEN
        vAmountChange := -pQuantity;
    ELSE
        RETURN;
    END IF;

    UPDATE "ProductAmounts"
    SET "Amount" = "Amount" + vAmountChange
    WHERE "BranchId" = pBranchId
      AND "ProductId" = pProductId;

    GET DIAGNOSTICS v_rowcount = ROW_COUNT;

    IF v_rowcount = 0 THEN
        INSERT INTO "ProductAmounts"
            ("BranchId", "ProductId", "Amount")
        VALUES
            (pBranchId, pProductId, vAmountChange);
    END IF;
END;
$$;


-- =========================================================
-- 3. Accountings INSERT trigger
-- =========================================================
DROP TRIGGER IF EXISTS ___accountings_insert ON "Accountings";

CREATE OR REPLACE FUNCTION trg_accountings_before_insert()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    PERFORM UpdateProductAmount(
        NEW."Type",
        NEW."BranchId",
        NEW."EventId",
        NEW."Quantity"
    );

    PERFORM UpdateBalance(
        NEW."FirmId",
        NEW."BranchId",
        NEW."CustomerId",
        NEW."SupplierId",
        -(NEW."Quantity" * NEW."Amount")
    );

    RETURN NEW;
END;
$$;

CREATE TRIGGER ___accountings_insert
BEFORE INSERT ON "Accountings"
FOR EACH ROW
EXECUTE FUNCTION trg_accountings_before_insert();


-- =========================================================
-- 4. Accountings UPDATE trigger
-- =========================================================
DROP TRIGGER IF EXISTS ___accountings_update ON "Accountings";

CREATE OR REPLACE FUNCTION trg_accountings_before_update()
RETURNS trigger
LANGUAGE plpgsql
AS $$
DECLARE
    lastQuantity NUMERIC(10,4) := 0;
    newQuantity  NUMERIC(10,4) := 0;
    lastAmount   NUMERIC(10,4) := 0;
    newAmount    NUMERIC(10,4) := 0;
BEGIN
    lastQuantity := OLD."Quantity";
    newQuantity  := NEW."Quantity";

    lastAmount := lastQuantity * OLD."Amount";
    newAmount  := newQuantity  * NEW."Amount";

    IF OLD."Status" <> 1 THEN
        lastQuantity := 0;
        lastAmount   := 0;
    END IF;

    IF NEW."Status" <> 1 THEN
        newQuantity := 0;
        newAmount   := 0;
    END IF;

    IF lastQuantity <> newQuantity THEN
        PERFORM UpdateProductAmount(
            NEW."Type",
            NEW."BranchId",
            NEW."EventId",
            newQuantity - lastQuantity
        );
    END IF;

    IF lastAmount <> newAmount THEN
        PERFORM UpdateBalance(
            NEW."FirmId",
            NEW."BranchId",
            NEW."CustomerId",
            NEW."SupplierId",
            lastAmount - newAmount
        );
    END IF;

    IF NEW."Type" IN (1,2) THEN
        IF OLD."Quantity" <> NEW."Quantity" THEN
            PERFORM UpdateProductAmount(
                NEW."Type",
                NEW."BranchId",
                NEW."EventId",
                NEW."Quantity" - OLD."Quantity"
            );
        END IF;
    END IF;

    RETURN NEW;
END;
$$;

CREATE TRIGGER ___accountings_update
BEFORE UPDATE ON "Accountings"
FOR EACH ROW
EXECUTE FUNCTION trg_accountings_before_update();


-- =========================================================
-- 5. Payments INSERT trigger
-- =========================================================
DROP TRIGGER IF EXISTS ___payments_insert ON "Payments";

CREATE OR REPLACE FUNCTION trg_payments_before_insert()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    IF NEW."IsDeleted" = 0 AND NEW."Status" = 1 THEN
        PERFORM UpdateBalance(
            NEW."FirmId",
            NEW."BranchId",
            NEW."CustomerId",
            NEW."SupplierId",
            NEW."Amount"
        );
    END IF;

    RETURN NEW;
END;
$$;

CREATE TRIGGER ___payments_insert
BEFORE INSERT ON "Payments"
FOR EACH ROW
EXECUTE FUNCTION trg_payments_before_insert();


-- =========================================================
-- 6. Payments UPDATE trigger
-- =========================================================
DROP TRIGGER IF EXISTS ___payments_update ON "Payments";

CREATE OR REPLACE FUNCTION trg_payments_before_update()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    IF OLD."Status" = 1 AND NEW."Status" <> 1 THEN
        PERFORM UpdateBalance(
            OLD."FirmId",
            OLD."BranchId",
            OLD."CustomerId",
            OLD."SupplierId",
            -OLD."Amount"
        );
    END IF;

    IF OLD."Status" <> 1 AND NEW."Status" = 1 THEN
        PERFORM UpdateBalance(
            NEW."FirmId",
            NEW."BranchId",
            NEW."CustomerId",
            NEW."SupplierId",
            NEW."Amount"
        );
    END IF;

    RETURN NEW;
END;
$$;

CREATE TRIGGER ___payments_update
BEFORE UPDATE ON "Payments"
FOR EACH ROW
EXECUTE FUNCTION trg_payments_before_update();


-- =========================================================
-- 7. Products INSERT trigger
-- =========================================================
DROP TRIGGER IF EXISTS ___productinsert ON "Products";

CREATE OR REPLACE FUNCTION trg_products_after_insert()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    INSERT INTO "ProductPriceHistories"
        ("ProductId", "Amount", "CreaterId", "Created")
    VALUES
        (NEW."Id", NEW."Price", NEW."CreaterId", NEW."Created");

    RETURN NEW;
END;
$$;

CREATE TRIGGER ___productinsert
AFTER INSERT ON "Products"
FOR EACH ROW
EXECUTE FUNCTION trg_products_after_insert();


-- =========================================================
-- 8. Products UPDATE trigger
-- =========================================================
DROP TRIGGER IF EXISTS ___productupdate ON "Products";

CREATE OR REPLACE FUNCTION trg_products_after_update()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    IF OLD."Price" <> NEW."Price" THEN
        INSERT INTO "ProductPriceHistories"
            ("ProductId", "Amount", "CreaterId", "Created")
        VALUES
            (NEW."Id", NEW."Price", NEW."UpdaterId", NEW."Created");
    END IF;

    RETURN NEW;
END;
$$;

CREATE TRIGGER ___productupdate
AFTER UPDATE ON "Products"
FOR EACH ROW
EXECUTE FUNCTION trg_products_after_update();
