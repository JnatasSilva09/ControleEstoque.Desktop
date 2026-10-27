# Decisões do Projeto (SaaS)

## Front-end
- **Tecnologia:** Blazor (com MudBlazor) ou React + TypeScript.
- **Motivo:** Criar uma interface web moderna mantendo boa produtividade.

## Banco de Dados & Infra
- **Banco:** PostgreSQL gerenciado (Neon / Supabase / Render).
- **Isolamento:** Multi-tenant em banco único usando `empresa_id` e Row Level Security (RLS).

## Pagamento
- **Gateway:** Asaas ou Mercado Pago.
- **Motivo:** Suporte nativo a Pix e boletos recorrentes no Brasil.