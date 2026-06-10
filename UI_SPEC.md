# Stoq — UI Specification

Documento de referência para redesign e melhorias de UI/UX. Use como insumo ao conversar com o Claude para gerar mockups, sugerir melhorias ou reprojetar fluxos.

---

## Design Tokens

| Token | Valor |
|---|---|
| Primary | `#1E3A5F` (azul escuro) |
| Secondary / Accent | `#2E7D32` (verde) |
| Background | surface claro / escuro (MD3) |
| Border radius padrão | ~12px (Paper Card) |
| Fonte | System default |
| Bottom tabs | 5 tabs: Estoque · Lista de compras · Scan (FAB central) · Catálogo · Perfil |

---

## Estrutura de Navegação

```
RootNavigator
├── AuthNavigator (unauthenticated)
│   ├── Login
│   ├── Register
│   ├── ForgotPassword
│   └── ResetPassword
│
├── HouseholdSetupNavigator (authenticated, no household)
│   ├── HouseholdSetup
│   ├── CreateHousehold
│   └── JoinHousehold
│
└── MainTabNavigator (authenticated + household)
    ├── InventoryTab
    │   ├── InventoryDashboard          ← tab root
    │   ├── ProductInventoryDetail
    │   ├── AddInventoryItem
    │   │   └── ProductSearch (modal)
    │   ├── ConsumeInventoryItem
    │   └── ExpiryAlerts
    │
    ├── ShoppingTab
    │   ├── ShoppingList                ← tab root
    │   ├── AddShoppingItem
    │   ├── ScanList (ReceiptScanList)
    │   ├── ReceiptScan
    │   └── ReceiptConfirm
    │
    ├── ScanTab (intercepted → CatalogTab/BarcodeScanner)
    │
    ├── CatalogTab
    │   ├── ProductList                 ← tab root
    │   ├── ProductDetail
    │   ├── ProductForm (create/edit)
    │   └── BarcodeScanner
    │
    └── ProfileTab
        ├── Profile                     ← tab root
        ├── HouseholdSettings
        └── ManageMembers
```

---

## Auth

### Login
**Rota:** `AuthStack > Login`

**Layout:**
- Header: título "Bem-vindo de volta" + subtítulo
- Campo: E-mail (outlined)
- Campo: Senha (outlined, toggle visibilidade)
- Link: "Esqueceu a senha?" (alinhado à direita)
- Botão primário: "Entrar" (full width, verde)
- Divisor: "Ou continue com"
- Botão outline: "Continuar com Google" (ícone G)
- Rodapé: "Não tem uma conta? Cadastre-se"

**Estados:**
- Idle
- Loading (botão com spinner, inputs desabilitados)
- Erro (mensagem inline abaixo dos campos)

**Navegação:**
- Entrar → `MainTabNavigator` (se tiver household) ou `HouseholdSetupNavigator`
- "Esqueceu a senha?" → `ForgotPassword`
- "Cadastre-se" → `Register`

---

### Register
**Rota:** `AuthStack > Register`

**Layout:**
- Header: "Criar conta" + subtítulo
- Campo: Nome de exibição
- Campo: E-mail
- Campo: Senha (toggle visibilidade)
- Botão primário: "Criar conta"
- Rodapé: "Já tem uma conta? Entrar"

**Validação (inline):**
- Nome obrigatório
- E-mail válido
- Senha: mín. 8 chars, 1 maiúscula, 1 dígito

**Navegação:**
- Sucesso → `HouseholdSetupNavigator`
- "Entrar" → `Login`

---

### ForgotPassword
**Rota:** `AuthStack > ForgotPassword`

**Layout (estado inicial):**
- Header + subtítulo
- Campo: E-mail
- Botão: "Enviar código"
- Link: "Voltar ao login"

**Layout (estado sucesso):**
- Ícone de confirmação
- Título: "Verifique seu e-mail"
- Subtítulo com e-mail mascarado
- Botão: vai para `ResetPassword`

---

### ResetPassword
**Rota:** `AuthStack > ResetPassword`

**Layout:**
- Campo: Código OTP (6 dígitos)
- Campo: Nova senha (toggle visibilidade)
- Botão: "Redefinir senha"

**Navegação:** Sucesso → `Login`

---

## Household Setup

### HouseholdSetup
**Rota:** `HouseholdSetupNavigator > HouseholdSetup`

**Layout:**
- Título + subtítulo descritivo
- Botão primário: "Criar residência"
- Botão outline: "Entrar em uma residência"

---

### CreateHousehold
**Rota:** `HouseholdSetupNavigator > CreateHousehold`

**Layout (estado formulário):**
- Campo: Nome da residência
- Segmented buttons: 🇧🇷 Brasil / 🇺🇸 Estados Unidos
- Botão: "Criar residência"

**Layout (estado sucesso):**
- Card com código de convite (grande, destacado)
- Texto de validade (48h)
- Botão outline: "Compartilhar código"
- Botão primário: "Continuar"

---

### JoinHousehold
**Rota:** `HouseholdSetupNavigator > JoinHousehold`

**Layout:**
- Campo: Código de convite (6 chars, auto-uppercase)
- Botão primário: "Entrar na residência"
- Botão outline: "Cancelar"

---

## Inventário

### InventoryDashboard
**Rota:** `InventoryTab > InventoryDashboard`
**Screenshots:** tela 2

**Layout:**
```
┌─────────────────────────────────┐
│ Estoque                         │  ← AppBar
├─────────────────────────────────┤
│ ⚠️  N item(s) próximo ao         │  ← Banner (condicional, clicável)
│    vencimento              >    │
├─────────────────────────────────┤
│ 🔍 Buscar itens…                │  ← Search bar
├─────────────────────────────────┤
│ 📦 Produto A                    │
│    X unid · Y lotes         >   │  ← Item row (chip "Estoque baixo" se aplicável)
│ 📦 Produto B                    │
│    X unid                   >   │
└─────────────────────────────────┘
                              [+]   ← FAB verde
```

**Estados:**
- Loading: ActivityIndicator centralizado
- Erro: texto de erro + botão "Tentar novamente"
- Vazio: título + subtítulo + botão "Adicionar primeiro item"
- Populated: lista agrupada por produto

**Lógica de agrupamento:** itens com mesmo `productId` são agrupados; exibe quantidade total e nº de lotes.

**Chip "Estoque baixo":** aparece quando `totalQuantity ≤ minQuantity`

**Navegação:**
- Banner → `ExpiryAlerts`
- Row → `ProductInventoryDetail`
- FAB → `AddInventoryItem`

---

### ProductInventoryDetail
**Rota:** `InventoryTab > ProductInventoryDetail`
**Screenshots:** tela 3

**Layout:**
```
← Arroz 7 grãos                    ← AppBar com título do produto

┌─────────────────────────────────┐
│ 2 units                         │  ← Total em destaque
│ Estoque mín.: 0 units           │
│ [+ Adicionar à lista de compras]│
└─────────────────────────────────┘

LOTES

│ 1 units          ⊖  ⊕  🗑      │  ← Batch row
│ Venc. 30/05/2026                │
│─────────────────────────────────│
│ 1 units          ⊖  ⊕  🗑      │
│ Venc. 20/06/2026                │

                    [+ Adicionar lote]  ← FAB expandido
```

**Ações por lote:**
- `⊖` → `ConsumeInventoryItem` (pré-selecionado)
- `⊕` → adiciona 1 unidade diretamente
- `🗑` → remove o lote (com confirmação)

**Navegação:**
- "Adicionar lote" → `AddInventoryItem` (produto pré-selecionado)
- "Adicionar à lista de compras" → cria item na lista + snackbar

---

### AddInventoryItem
**Rota:** `InventoryTab > AddInventoryItem`

**Layout:**
```
← Adicionar ao estoque

[Produto]           ← picker (tap → ProductSearch) ou campo locked
[Quantidade]
[Unidade]
[Data de validade]  ← auto-formatação BR: DD/MM/AAAA / US: MM/DD/YYYY
[Observações]       ← opcional

[Adicionar ao estoque]  ← botão primário
```

**Validação:** produto, quantidade numérica ≥ 0, unidade, data válida e não no passado.

**Navegação:**
- Picker de produto → `ProductSearch` (modal)

---

### ProductSearch
**Rota:** `InventoryTab > ProductSearch` (modal)

**Layout:**
- SearchBar no topo
- Lista paginada (20/página, scroll para carregar mais)
- Ícone `home-outline` para produtos da residência vs catálogo geral

**Trigger de busca:** mín. 4 caracteres

---

### ConsumeInventoryItem
**Rota:** `InventoryTab > ConsumeInventoryItem`

**Layout:**
```
← Consumir item

┌─────────────────────────────────┐
│ Em estoque: X units             │  ← Card informativo
└─────────────────────────────────┘

[Quantidade a consumir]   [unit]   ← input + affix

[Consumir]    [Cancelar]
```

**Validação:** quantidade > 0 e ≤ estoque disponível.

---

### ExpiryAlerts
**Rota:** `InventoryTab > ExpiryAlerts`
**Screenshots:** tela 4

**Layout:**
```
← Alertas de Validade

Produtos vencendo    [Verificar produtos
nos próximos 7 dias.  a vencer]          ← chip/button

Produto A   1 units       [Vencido]       ← chip vermelho
Produto B   2 units       [1 dia restante] ← chip terciário
Produto C   1 units       [5 dias restantes] ← chip outline
```

**Cores dos chips:**
- `≤ 0 dias` (vencido): vermelho (`error`)
- `≤ 1 dia`: vermelho
- `≤ 3 dias`: terciário
- `> 3 dias`: outline neutro

**Pull-to-refresh:** disponível

---

## Lista de Compras

### ShoppingList
**Rota:** `ShoppingTab > ShoppingList`
**Screenshots:** telas 5, 6

**Layout:**
```
Lista de compras              [Limpar marcados]  ← condicional

□  Produto A          🗑
   1 units
────────────────────────────
☑  Produto B          🗑      ← item marcado (strikethrough + opaco)
   2 units

                          [+]  ← FAB.Group
```

**FAB.Group expandido (tela 6):**
- "Escanear cupom" (ícone receipt)
- "Adicionar item" (ícone +)
- ✕ (fechar)

**Chip "Adicionado auto.":** aparece em itens com `source === 'Auto'`

**Navegação:**
- Checkbox → toggle check/uncheck (otimista)
- 🗑 → remove item
- "Limpar marcados" → remove todos os checked
- FAB > "Adicionar item" → `AddShoppingItem`
- FAB > "Escanear cupom" → `ReceiptScan`

---

### AddShoppingItem
**Rota:** `ShoppingTab > AddShoppingItem`
**Screenshots:** tela 7

**Layout:**
```
← Adicionar item

[Nome do produto]

[Quantidade]    [Unidade]       ← row 2 colunas

[Observações]

[Adicionar à lista]
```

---

### ReceiptScan
**Rota:** `ShoppingTab > ReceiptScan`

**Layout:**
```
┌─────────────────────────────────┐
│                                 │
│         [câmera ativa]          │
│   QR NF-e detectado → processa  │
│   automaticamente               │
│                                 │
│  [📷 Fotografar cupom] [Cancelar]│
└─────────────────────────────────┘
```

**Fluxo:**
1. QR NF-e detectado → POST API → aceito → navega para `ScanList` (status "Aguardando")
2. Foto capturada → envia imagem → navega para `ReceiptConfirm`

**Estados:** permissão negada → UI de solicitação; processando → loading overlay; erro → Snackbar.

---

### ScanList (ReceiptScanList)
**Rota:** `ShoppingTab > ScanList`

**Layout:**
```
← Histórico de Escaneamentos

┌─────────────────────────────────┐
│ VILLA COMERCIO…                 │
│ há 2 min · 12 itens    [Aguardando 🔄]│
└─────────────────────────────────┘
┌─────────────────────────────────┐
│ SUPERMERCADO X                  │
│ há 1h · 8 itens    [Pronto para confirmar]│
└─────────────────────────────────┘

                    [Escanear Cupom]  ← botão inferior
```

**Status badges:**
| Status | Cor | Ícone |
|---|---|---|
| pending | neutro | ⏳ |
| processing | primary | 🔄 spinner |
| parsed | secondary | ✓ clicável |
| confirmed | success | ✓ |
| failed | error | ✗ |

**Polling:** atualiza a cada 30s automaticamente.

**Navegação:** tap em card `parsed` → `ReceiptConfirm`

---

### ReceiptConfirm
**Rota:** `ShoppingTab > ReceiptConfirm`
**Screenshots:** tela 8

**Layout:**
```
← Confirmar itens

VILLA COMERCIO E DISTRIBUICAO…      ← header verde (nome do varejista)
Revise e ajuste os itens antes
de adicioná-los ao estoque.

┌─────────────────────────────────┐
│ Nome do produto                 │ ✕
│ [Manga palmer              ]    │
│ [0.505]    [Quilogramas    ]    │
│ [Data de validade (opcional)]   │
└─────────────────────────────────┘
┌─────────────────────────────────┐
│ [Vinagrete 200g            ] ✕  │
│ [1    ]    [Unidades       ]    │
│ [Data de validade (opcional)]   │
└─────────────────────────────────┘

[✓ Adicionar ao estoque]  ← botão primário fixo no rodapé
```

**Estado "já confirmado":**
- UI bloqueada com aviso
- Botão desabilitado

**Estado "sem itens":**
- Mensagem informativa

---

## Catálogo

### ProductList
**Rota:** `CatalogTab > ProductList`
**Screenshots:** tela 9

**Layout:**
```
Catálogo de produtos          [≡ filtro]

🔍 Buscar produtos…

[Todos] [Gerais] [Minha casa]   ← filter chips

📦 Produto A                   ← item
📦 Produto B

                          [+]   ← FAB.Group
```

**FAB.Group:**
- "Escanear código de barras"
- "Adicionar manualmente"

**Filtros:**
- **Todos:** busca obrigatória (≥ 4 chars)
- **Gerais:** busca obrigatória
- **Minha casa:** exibe sem busca, com botões editar/deletar

**Navegação:** item → `ProductDetail`; FAB scan → `BarcodeScanner`; FAB manual → `ProductForm`

---

### ProductDetail
**Rota:** `CatalogTab > ProductDetail`

**Layout:**
```
← [Nome do produto]    [✏️] [🗑]  ← botões só para produtos da residência

┌─────────────────────────────────┐
│ [imagem ou ícone]               │
│ Nome do produto                 │
│ Marca                           │
│ [Categoria]  [Unidade]          │ ← chips
│ Qtd. mín: X                     │
│ Código de barras: XXXX          │
└─────────────────────────────────┘

[Adicionar ao estoque]
```

---

### ProductForm
**Rota:** `CatalogTab > ProductForm`
**Screenshots:** tela 10

**Layout:**
```
← Novo produto / Editar produto

[Nome do produto]
[Marca]              ← opcional
[Categoria     ▼]    ← dropdown
[Unidade       ▼]    ← dropdown
[Qtd. mínima]
[Código de barras]  [🔲]  ← ícone abre BarcodeScanner
[URL da imagem]

[Cancelar]  [Criar / Salvar]
```

**Dropdowns de categoria e unidade:** bilíngue (PT/EN)

---

### BarcodeScanner
**Rota:** `CatalogTab > BarcodeScanner`

**Layout:**
```
┌─────────────────────────────────┐
│         [câmera]                │
│   ┌───────────────────┐         │
│   │   viewfinder 260px│         │ ← overlay de scan
│   └───────────────────┘         │
│   "Aponte para o código"        │
│                                 │
│         [Cancelar]              │
└─────────────────────────────────┘
```

**Formatos suportados:** EAN-13, EAN-8, UPC-A, UPC-E, Code128, Code39

**Fluxo:** código detectado → lookup Open Food Facts → produto encontrado → `ProductDetail`; não encontrado → `ProductForm` (pré-preenchido com barcode)

---

## Perfil

### Profile
**Rota:** `ProfileTab > Profile`
**Screenshots:** tela 11

**Layout:**
```
┌─────────────────────────────────┐
│         [Avatar / Iniciais]     │
│           Renato                │
│  renato.andrade575@gmail.com    │
└─────────────────────────────────┘

🏠 Configurações da residência >
   Código de convite, membros e endereço

[→ Sair]                           ← vermelho
```

**Navegação:**
- "Configurações da residência" → `HouseholdSettings`
- "Sair" → dialog de confirmação → logout → `AuthNavigator`

---

### HouseholdSettings
**Rota:** `ProfileTab > HouseholdSettings`

**Layout:**
```
← Configurações da residência

CÓDIGO DE CONVITE
┌─────────────────────────────────┐
│  ABC123                         │  ← código destacado
│  Expira em 12/06/2026           │
└─────────────────────────────────┘
[Compartilhar]  [Regenerar]   ← "Regenerar" só para Admin

─────────────────────────────────
🧑‍🤝‍🧑 Gerenciar membros          >   ← só para Admin
   2 membro(s)
```

---

### ManageMembers
**Rota:** `ProfileTab > ManageMembers`

**Layout:**
```
← Gerenciar membros

┌─────────────────────────────────┐
│ [R] Renato          [Admin] ✏️ 🗑│  ← você
│ [M] Maria           [Membro] ✏️ 🗑│
└─────────────────────────────────┘
```

**Chips de role:**
- Admin: cor `primary`
- Membro: cor `secondary`
- Viewer: cor `outline`

**Ações (admin only):**
- ✏️ → dialog de seleção de role
- 🗑 → dialog de confirmação de remoção

---

## Fluxo NF-e (fim a fim)

```
ShoppingList
  └─ FAB > "Escanear cupom"
       └─ ReceiptScan
            ├─ QR NF-e detectado
            │    └─ POST /receipts/nfe → 202 Accepted
            │         └─ ScanList (status: Aguardando → Pronto)
            │              └─ tap quando Parsed → ReceiptConfirm
            │                   └─ "Adicionar ao estoque" → InventoryDashboard
            │
            └─ Foto capturada
                 └─ POST /receipts/photo → processa sync
                      └─ ReceiptConfirm
                           └─ "Adicionar ao estoque" → InventoryDashboard
```

---

## Fluxo de Notificações Push

| Evento | Destino deep link |
|---|---|
| Item próximo ao vencimento | `InventoryTab > ExpiryAlerts` |
| Estoque baixo | `ShoppingTab > ShoppingList` |
| Item adicionado à lista por outro membro | `ShoppingTab > ShoppingList` |
| NF-e processada (app em background) | `ShoppingTab > ScanList` |
| NF-e falhou (app em background) | `ShoppingTab > ScanList` |

---

## Observações para Redesign

### Pontos de melhoria identificados nas screenshots

1. **Nomes em caixa alta** ("MOSTARDA HELLMANNS C") — vêm da NF-e sem tratamento; considerar `text-transform: capitalize` ou normalização no backend
2. **Unidade "units"** em inglês — tradução incompleta para PT-BR; deveria exibir "unidades"
3. **Ícone genérico de geladeira** para todos os produtos — oportunidade de usar imagem do produto quando disponível
4. **Banner de validade** poderia mostrar número de dias e produto mais urgente diretamente, sem precisar navegar
5. **Tela de Perfil** muito esparsa — oportunidade para adicionar configurações de notificação, tema e idioma
6. **ReceiptConfirm** tem muitos campos abertos simultaneamente — considerar accordion por item ou scroll com sticky header por produto
7. **Bottom tab "Catálogo de..."** texto cortado — label longo demais para o espaço

### Componentes reutilizáveis candidatos a design system
- `StatusBadge` (usado em ScanList)
- `ExpiryChip` (usado em ExpiryAlerts e InventoryDetail)
- `InviteCodeCard` (usado em CreateHousehold e HouseholdSettings)
- `ProductRow` (usado em InventoryDashboard e ProductList)
- `BatchCard` (usado em ProductInventoryDetail)
- `OfflineBanner` (global, acima do navigator)
