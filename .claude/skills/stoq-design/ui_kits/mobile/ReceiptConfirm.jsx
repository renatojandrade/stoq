/* Stoq UI Kit — improved ReceiptConfirmScreen (addresses "dense" feedback) */
function ReceiptConfirmScreen({ scan, nav, snack }) {
  const [items, setItems] = useState(window.STOQ.receiptItems);
  const c = window.STOQ.c, f = window.STOQ.font;
  const remove = id => setItems(items.filter(i=>i.id!==id));
  const retailer = (scan && scan.retailer) || 'Supermercado Pão de Açúcar';

  return (
    <div style={{ flex:1, display:'flex', flexDirection:'column', position:'relative', background:c.surface2 }}>
      {/* Summary header */}
      <div style={{ background:c.primary, padding:'16px 18px 18px', flex:'none' }}>
        <div style={{ display:'flex', alignItems:'center', gap:10 }}>
          <Icon name="store-outline" size={20} color="#fff" />
          <span style={{ font:`500 16px ${f}`, color:'#fff', flex:1 }}>{retailer}</span>
        </div>
        <span style={{ font:`400 13px ${f}`, color:'#CDE8D9', marginTop:6, display:'block' }}>
          {items.length} itens reconhecidos · revise antes de adicionar ao estoque
        </span>
      </div>

      <div style={{ flex:1, overflowY:'auto', padding:'14px 16px 100px', display:'flex', flexDirection:'column', gap:12 }}>
        {items.map(it => (
          <div key={it.id} style={{ background:c.background, borderRadius:14, padding:'14px 14px 12px', boxShadow:'0 1px 3px rgba(0,0,0,0.10)' }}>
            {/* top row: match status + remove */}
            <div style={{ display:'flex', alignItems:'center', gap:8, marginBottom:10 }}>
              {it.matched
                ? <Chip bg={c.secondaryContainer} fg={c.onSecondaryContainer}><Icon name="check-circle" size={13} color={c.onSecondaryContainer} />Reconhecido</Chip>
                : <Chip bg={c.warningContainer} fg={c.onWarningContainer}><Icon name="help-circle-outline" size={13} color={c.onWarningContainer} />Novo produto</Chip>}
              <div style={{ flex:1 }} />
              <IconButton name="close" size={18} onClick={()=>remove(it.id)} />
            </div>

            {/* product name field */}
            <CField label="Nome do produto" value={titleCase(it.name)} />

            {/* qty + unit */}
            <div style={{ display:'flex', gap:10, marginTop:10 }}>
              <div style={{ width:96 }}><CField label="Qtd." value={it.qty} /></div>
              <div style={{ flex:1 }}><CField label="Unidade" value={it.unit} /></div>
            </div>

            {/* optional expiry — de-emphasized affordance */}
            <button style={{ marginTop:12, display:'flex', alignItems:'center', gap:8, background:'transparent', border:'none',
              cursor:'pointer', padding:'4px 2px', color:c.primary, font:`500 13px ${f}` }}>
              <Icon name="calendar-plus" size={18} color={c.primary} />
              Adicionar validade (opcional)
            </button>
          </div>
        ))}
      </div>

      {/* sticky footer */}
      <div style={{ position:'absolute', left:0, right:0, bottom:0, padding:'14px 16px 28px', background:c.background, boxShadow:'0 -1px 5px rgba(0,0,0,0.10)' }}>
        <Btn variant="contained" full icon="check" onClick={()=>{ snack && snack('Itens adicionados ao seu estoque.'); nav('inventory'); }}>
          Adicionar {items.length} ao estoque
        </Btn>
      </div>
    </div>
  );
}

/* compact filled field used inside receipt cards */
function CField({ label, value }) {
  const c = window.STOQ.c, f = window.STOQ.font;
  return (
    <div style={{ background:c.surface2, borderRadius:8, padding:'6px 12px 7px', borderBottom:`2px solid ${c.outlineVariant}` }}>
      <div style={{ font:`400 11px ${f}`, color:c.onSurfaceVariant }}>{label}</div>
      <div style={{ font:`400 15px ${f}`, color:c.onSurface, marginTop:1 }}>{value}</div>
    </div>
  );
}
function titleCase(s) {
  return s.toLowerCase().replace(/\b\w/g, m=>m.toUpperCase()).replace(/\d+L\b/i, m=>m.toUpperCase());
}

Object.assign(window, { ReceiptConfirmScreen });
