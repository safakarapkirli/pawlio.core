-- Burası UI ile gönderilmeli, .Net ile gönderince hata veriyor

CREATE TABLE IF NOT EXISTS __History (
    id            BIGSERIAL PRIMARY KEY,
    table_name    TEXT NOT NULL,
    operation     CHAR(1) NOT NULL, -- I / U / D
    record_id     TEXT,
    changed_data  JSONB,
    old_data      JSONB,
    new_data      JSONB,
    changed_at    TIMESTAMPTZ NOT NULL DEFAULT now(),
    changed_by    INT
);

CREATE OR REPLACE FUNCTION trg_generic_history()
RETURNS trigger
LANGUAGE plpgsql
AS $$
DECLARE
    diff JSONB := '{}'::jsonb;
    k TEXT;
BEGIN
    -- INSERT
    IF TG_OP = 'INSERT' THEN
        INSERT INTO __History (
            table_name,
            operation,
            record_id,
            new_data
        )
        VALUES (
            TG_TABLE_NAME,
            'I',
            NEW.id::TEXT,
            to_jsonb(NEW)
        );
        RETURN NEW;
    END IF;

    -- DELETE
    IF TG_OP = 'DELETE' THEN
        INSERT INTO __History (
            table_name,
            operation,
            record_id,
            old_data
        )
        VALUES (
            TG_TABLE_NAME,
            'D',
            OLD.id::TEXT,
            to_jsonb(OLD)
        );
        RETURN OLD;
    END IF;

    -- UPDATE (sadece değişen alanlar)
    FOR k IN
        SELECT key
        FROM jsonb_object_keys(to_jsonb(NEW)) AS key
    LOOP
        IF to_jsonb(OLD)->k IS DISTINCT FROM to_jsonb(NEW)->k THEN
            diff := diff || jsonb_build_object(
                k,
                jsonb_build_object(
                    'old', to_jsonb(OLD)->k,
                    'new', to_jsonb(NEW)->k
                )
            );
        END IF;
    END LOOP;

    IF diff <> '{}'::jsonb THEN
        INSERT INTO __History (
            table_name,
            operation,
            record_id,
            changed_data
        )
        VALUES (
            TG_TABLE_NAME,
            'U',
            NEW.id::TEXT,
            diff
        );
    END IF;

    RETURN NEW;
END;
$$;



-- Trigger ları tek tek çalıştırmak gerekiyor

CREATE OR REPLACE TRIGGER trg_appointments_history
AFTER INSERT OR UPDATE OR DELETE
ON "Appointments"
FOR EACH ROW
EXECUTE FUNCTION trg_generic_history();

CREATE OR REPLACE TRIGGER trg_balances_history
AFTER INSERT OR UPDATE OR DELETE
ON "Balances"
FOR EACH ROW
EXECUTE FUNCTION trg_generic_history();

CREATE OR REPLACE TRIGGER trg_baskets_history
AFTER INSERT OR UPDATE OR DELETE
ON "Baskets"
FOR EACH ROW
EXECUTE FUNCTION trg_generic_history();

CREATE OR REPLACE TRIGGER trg_branches_history
AFTER INSERT OR UPDATE OR DELETE
ON "Branches"
FOR EACH ROW
EXECUTE FUNCTION trg_generic_history();

CREATE OR REPLACE TRIGGER trg_customers_history
AFTER INSERT OR UPDATE OR DELETE
ON "Customers"
FOR EACH ROW
EXECUTE FUNCTION trg_generic_history();

CREATE OR REPLACE TRIGGER trg_definitions_history
AFTER INSERT OR UPDATE OR DELETE
ON "Definitions"
FOR EACH ROW
EXECUTE FUNCTION trg_generic_history();

CREATE OR REPLACE TRIGGER trg_firms_history
AFTER INSERT OR UPDATE OR DELETE
ON "Firms"
FOR EACH ROW
EXECUTE FUNCTION trg_generic_history();

CREATE OR REPLACE TRIGGER trg_payments_history
AFTER INSERT OR UPDATE OR DELETE
ON "Payments"
FOR EACH ROW
EXECUTE FUNCTION trg_generic_history();

CREATE OR REPLACE TRIGGER trg_products_history
AFTER INSERT OR UPDATE OR DELETE
ON "Products"
FOR EACH ROW
EXECUTE FUNCTION trg_generic_history();

CREATE OR REPLACE TRIGGER trg_productamounts_history
AFTER INSERT OR UPDATE OR DELETE
ON "ProductAmounts"
FOR EACH ROW
EXECUTE FUNCTION trg_generic_history();

CREATE OR REPLACE TRIGGER trg_productpricehistories_history
AFTER INSERT OR UPDATE OR DELETE
ON "ProductPriceHistories"
FOR EACH ROW
EXECUTE FUNCTION trg_generic_history();

CREATE OR REPLACE TRIGGER trg_suppliers_history
AFTER INSERT OR UPDATE OR DELETE
ON "Suppliers"
FOR EACH ROW
EXECUTE FUNCTION trg_generic_history();

CREATE OR REPLACE TRIGGER trg_users_history
AFTER INSERT OR UPDATE OR DELETE
ON "Users"
FOR EACH ROW
EXECUTE FUNCTION trg_generic_history();



