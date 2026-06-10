/* Stoq UI Kit — screens. Depends on components.jsx + theme.js */
const S = window.STOQ;
const CC = window.STOQ.c;
const F = window.STOQ.font;

/* ============ LOGIN ============ */
function LoginScreen({ nav }) {
  const [show, setShow] = useState(false);
  return (
    <div style={{ flex:1, display:'flex', flexDirection:'column', justifyContent:'center', padding:'0 24px', background:CC.background }}>
      <div style={{ display:'flex', flexDirection:'column', alignItems:'center', marginBottom:28 }}>
        <img src="../../assets/stoq-mark.svg" alt="Stoq" style={{ width:64, height:64, marginBottom:14 }} />
        <span style={{ font:`400 28px ${F}`, color:CC.onSurface }}>Bem-vindo de volta</span>
        <span style={{ font:`400 14px ${F}`, color:CC.onSurfaceVariant, marginTop:4 }}>Entre na sua conta</span>
      </div>
      <Field label="E-mail" value="ana.silva@email.com" />
      <Field label="Senha" value={show?'minhasenha':'••••••••••'} right={
        <IconButton name={show?'eye-off':'eye'} onClick={()=>setShow(s=>!s)} /> } />
      <div style={{ alignSelf:'flex-end', margin:'2px 0 6px' }}><Btn variant="text">Esqueceu a senha?</Btn></div>
      <Btn variant="contained" full onClick={()=>nav('inventory')}>Entrar</Btn>
      <div style={{ height:1, background:CC.outlineVariant, margin:'22px 0' }} />
      <Btn variant="outlined" full icon="google" onClick={()=>nav('inventory')}>Continuar com Google</Btn>
      <div style={{ display:'flex', justifyContent:'center', alignItems:'center', marginTop:14 }}>
        <span style={{ font:`400 14px ${F}`, color:CC.onSurface }}>Não tem uma conta? </span>
        <Btn variant="text">Cadastre-se</Btn>
      </div>
    </div>
  );
}
function Field({ label, value, right }) {
  return (
    <div style={{ position:'relative', marginBottom:14 }}>
      <div style={{ position:'absolute', top:-8, left:10, background:CC.background, padding:'0 5px',
        font:`400 12px ${F}`, color:CC.onSurfaceVariant, zIndex:1 }}>{label}</div>
      <div style={{ height:52, border:`1px solid ${CC.outline}`, borderRadius:4, display:'flex', alignItems:'center', padding:'0 6px 0 14px' }}>
        <span style={{ flex:1, font:`400 16px ${F}`, color:CC.onSurface }}>{value}</span>{right}
      </div>
    </div>
  );
}

/* ============ INVENTORY DASHBOARD ============ */
function InventoryScreen({ nav }) {
  const [q, setQ] = useState('');
  const exp = S.expiring.length;
  return (
    <div style={{ flex:1, display:'flex', flexDirection:'column', position:'relative', background:CC.background, overflow:'hidden' }}>
      <Banner icon="calendar-alert" tone="alert" onClick={()=>nav('expiry')}>{exp} itens próximos ao vencimento</Banner>
      <SearchBar placeholder="Buscar itens…" value={q} onChange={setQ} />
      <div style={{ flex:1, overflowY:'auto', paddingBottom:90 }}>
        {S.inventory.map((it,idx)=>(
          <div key={it.id}>
            <div onClick={()=>nav('detail', it)} style={{ display:'flex', alignItems:'center', gap:14, padding:'12px 16px', cursor:'pointer' }}>
              <Thumb icon={it.icon} tint={it.tint} />
              <div style={{ flex:1, display:'flex', flexDirection:'column', gap:3 }}>
                <span style={{ font:`400 16px ${F}`, color:CC.onSurface }}>{it.name}</span>
                <span style={{ font:`400 12px ${F}`, color:CC.onSurfaceVariant }}>
                  {it.brand ? it.brand+' · ' : ''}{it.qty} {it.unit}{it.batches>1?`  ·  ${it.batches} lotes`:''}
                </span>
                {it.low && <Chip bg={CC.errorContainer} fg={CC.onErrorContainer}>Estoque baixo</Chip>}
              </div>
              <Icon name="chevron-right" size={20} />
            </div>
            {idx<S.inventory.length-1 && <Divider inset={76} />}
          </div>
        ))}
      </div>
      <Fab icon="plus" onClick={()=>nav('addform')} />
    </div>
  );
}

/* ============ EXPIRY ALERTS ============ */
function ExpiryScreen() {
  function chip(days) {
    if (days<=1) return { label: days===0?'Vence hoje':'1 dia restante', col:CC.error };
    if (days<=3) return { label:`${days} dias restantes`, col:CC.warning };
    return { label:`${days} dias restantes`, col:CC.outline };
  }
  return (
    <div style={{ flex:1, display:'flex', flexDirection:'column', background:CC.background }}>
      <div style={{ display:'flex', alignItems:'center', gap:12, padding:'12px 16px' }}>
        <span style={{ flex:1, font:`400 12px ${F}`, color:CC.onSurfaceVariant }}>Produtos vencendo nos próximos 7 dias.</span>
        <Btn variant="tonal">Verificar a vencer</Btn>
      </div>
      <Divider />
      <div style={{ flex:1, overflowY:'auto' }}>
        {S.expiring.map((it,idx)=>{ const ch=chip(it.days); return (
          <div key={it.id}>
            <div style={{ display:'flex', alignItems:'center', gap:14, padding:'12px 16px' }}>
              <Thumb icon={it.icon} tint={it.tint} size={42} />
              <div style={{ flex:1, display:'flex', flexDirection:'column', gap:2 }}>
                <span style={{ font:`400 16px ${F}`, color:CC.onSurface }}>{it.name}</span>
                <span style={{ font:`400 12px ${F}`, color:CC.onSurfaceVariant }}>{it.qty} · {it.brand}</span>
              </div>
              <Chip outline fg={ch.col}>{ch.label}</Chip>
            </div>
            {idx<S.expiring.length-1 && <Divider inset={72} />}
          </div> ); })}
      </div>
    </div>
  );
}

/* ============ PRODUCT INVENTORY DETAIL ============ */
function DetailScreen({ product, snack, nav }) {
  const p = product || S.inventory[0];
  const batches = [
    { id:'b1', qty:`${p.qty} ${p.unit}`, exp:'Venc. 12/06/2026' },
    { id:'b2', qty:`1 ${p.unit}`, exp:'Sem data de validade' },
  ];
  return (
    <div style={{ flex:1, display:'flex', flexDirection:'column', position:'relative', background:CC.background }}>
      <div style={{ margin:16, background:CC.surface, borderRadius:12, padding:16, boxShadow:'0 1px 3px rgba(0,0,0,0.12)',
        display:'flex', gap:14, alignItems:'flex-start' }}>
        <Thumb icon={p.icon} tint={p.tint} size={56} />
        <div style={{ flex:1, display:'flex', flexDirection:'column', gap:4 }}>
          <div style={{ display:'flex', alignItems:'center', gap:10, flexWrap:'wrap' }}>
            <span style={{ font:`400 24px ${F}`, color:CC.onSurface }}>{p.qty} {p.unit}</span>
            {p.low && <Chip bg={CC.errorContainer} fg={CC.onErrorContainer}>Estoque baixo</Chip>}
          </div>
          {p.brand && <span style={{ font:`400 14px ${F}`, color:CC.onSurfaceVariant }}>{p.brand}</span>}
          <span style={{ font:`400 12px ${F}`, color:CC.onSurfaceVariant }}>Estoque mín.: {p.min} {p.unit}</span>
          <div style={{ marginTop:8 }}><Btn variant="outlined" icon="cart-plus" onClick={snack}>Adicionar à lista de compras</Btn></div>
        </div>
      </div>
      <div style={{ padding:'0 16px 8px', font:`500 14px ${F}`, color:CC.onSurfaceVariant, textTransform:'uppercase', letterSpacing:0.5 }}>Lotes</div>
      <div style={{ flex:1, overflowY:'auto', paddingBottom:90 }}>
        {batches.map((b,idx)=>(
          <div key={b.id}>
            <div style={{ display:'flex', alignItems:'center', padding:'12px 16px' }}>
              <div style={{ flex:1, display:'flex', flexDirection:'column', gap:2 }}>
                <span style={{ font:`400 16px ${F}`, color:CC.onSurface }}>{b.qty}</span>
                <span style={{ font:`400 12px ${F}`, color:CC.onSurfaceVariant }}>{b.exp}</span>
              </div>
              <IconButton name="minus-circle-outline" size={22} />
              <IconButton name="plus-circle-outline" size={22} />
              <IconButton name="trash-can-outline" size={22} color={CC.error} />
            </div>
            {idx<batches.length-1 && <Divider inset={16} />}
          </div>
        ))}
      </div>
      <Fab icon="plus" label="Adicionar lote" onClick={()=>nav&&nav('addform')} />
    </div>
  );
}

/* ============ SHOPPING LIST ============ */
function ShoppingScreen({ nav }) {
  const [items, setItems] = useState(S.shopping);
  const [fabOpen, setFabOpen] = useState(false);
  const toggle = id => setItems(items.map(i=> i.id===id?{...i, checked:!i.checked}:i));
  const remove = id => setItems(items.filter(i=>i.id!==id));
  const hasChecked = items.some(i=>i.checked);
  const ordered = [...items.filter(i=>!i.checked), ...items.filter(i=>i.checked)];
  if (items.length===0) return (
    <div style={{ flex:1, display:'flex', background:CC.background }}>
      <EmptyState icon="cart-outline" title="Sua lista de compras está vazia" subtitle="Adicione itens para comprar no mercado."
        action={<><Btn variant="contained" full icon="plus">Adicionar primeiro item</Btn>
          <Btn variant="outlined" full icon="receipt-text-outline" onClick={()=>nav('scanlist')}>Escanear cupom</Btn></>} />
    </div>
  );
  return (
    <div style={{ flex:1, display:'flex', flexDirection:'column', position:'relative', background:CC.background }}>
      {hasChecked && <div style={{ alignSelf:'flex-end', padding:'6px 8px 0' }}><Btn variant="text" icon="check-all" color={CC.error}>Limpar marcados</Btn></div>}
      <div style={{ flex:1, overflowY:'auto', paddingBottom:90 }}>
        {ordered.map((it,idx)=>(
          <div key={it.id}>
            <div onClick={()=>toggle(it.id)} style={{ display:'flex', alignItems:'center', gap:12, padding:'10px 8px 10px 14px', cursor:'pointer', background:CC.surface }}>
              <Checkbox on={it.checked} />
              <Thumb icon={it.icon} tint={it.tint} size={40} />
              <div style={{ flex:1, display:'flex', flexDirection:'column', gap:3, paddingTop:2 }}>
                <span style={{ font:`400 16px ${F}`, color:CC.onSurface, textDecoration: it.checked?'line-through':'none', opacity: it.checked?0.5:1 }}>{it.name}</span>
                <span style={{ font:`400 12px ${F}`, color:CC.onSurfaceVariant }}>{it.qty}</span>
                {it.source==='Auto' && <Chip bg={CC.secondaryContainer} fg={CC.onSecondaryContainer}>Adicionado auto.</Chip>}
              </div>
              <IconButton name="trash-can-outline" size={20} onClick={(e)=>{e.stopPropagation();remove(it.id);}} />
            </div>
            {idx<ordered.length-1 && <Divider inset={16} />}
          </div>
        ))}
      </div>
      {fabOpen && (
        <div style={{ position:'absolute', right:16, bottom:84, display:'flex', flexDirection:'column', alignItems:'flex-end', gap:12, zIndex:20 }}>
          <SpeedAction label="Escanear cupom" icon="receipt-text-outline" onClick={()=>nav('scanlist')} />
          <SpeedAction label="Adicionar item" icon="plus" />
        </div>
      )}
      <button onClick={()=>setFabOpen(o=>!o)} style={{ position:'absolute', right:16, bottom:20, width:56, height:56, borderRadius:16,
        background:CC.primaryContainer, border:'none', cursor:'pointer', display:'flex', alignItems:'center', justifyContent:'center', boxShadow:'0 3px 6px rgba(0,0,0,0.22)', zIndex:21 }}>
        <Icon name={fabOpen?'close':'plus'} size={24} color={CC.onPrimaryContainer} />
      </button>
      {fabOpen && <div onClick={()=>setFabOpen(false)} style={{ position:'absolute', inset:0, background:'rgba(0,0,0,0.04)', zIndex:10 }} />}
    </div>
  );
}
function SpeedAction({ label, icon, onClick }) {
  return (
    <div onClick={onClick} style={{ display:'flex', alignItems:'center', gap:10, cursor:'pointer' }}>
      <span style={{ background:CC.surface, padding:'5px 10px', borderRadius:6, font:`500 13px ${F}`, color:CC.onSurface, boxShadow:'0 1px 3px rgba(0,0,0,0.18)' }}>{label}</span>
      <div style={{ width:42, height:42, borderRadius:14, background:CC.surface, display:'flex', alignItems:'center', justifyContent:'center', boxShadow:'0 1px 3px rgba(0,0,0,0.2)' }}>
        <Icon name={icon} size={20} color={CC.primary} />
      </div>
    </div>
  );
}
function Checkbox({ on }) {
  return (
    <div style={{ width:22, height:22, borderRadius:4, border:`2px solid ${on?CC.primary:CC.outline}`, background: on?CC.primary:'transparent',
      display:'flex', alignItems:'center', justifyContent:'center', flex:'none' }}>
      {on && <Icon name="check" size={15} color="#fff" />}
    </div>
  );
}

/* ============ CATALOG ============ */
function CatalogScreen({ nav }) {
  const [filter, setFilter] = useState('all');
  const filtered = S.catalog.filter(c=> filter==='all' ? true : filter==='general' ? c.ref : !c.ref);
  return (
    <div style={{ flex:1, display:'flex', flexDirection:'column', position:'relative', background:CC.background }}>
      <SearchBar placeholder="Buscar produtos…" />
      <div style={{ display:'flex', gap:8, padding:'0 16px 8px' }}>
        {[['all','Todos'],['general','Gerais'],['household','Minha casa']].map(([k,l])=>(
          <button key={k} onClick={()=>setFilter(k)} style={{ height:32, padding:'0 14px', borderRadius:999, cursor:'pointer',
            border:`1px solid ${filter===k?CC.primary:CC.outlineVariant}`, background: filter===k?CC.primaryContainer:'transparent',
            color: filter===k?CC.onPrimaryContainer:CC.onSurfaceVariant, font:`500 13px ${F}` }}>{l}</button>
        ))}
      </div>
      <div style={{ flex:1, overflowY:'auto', paddingBottom:90 }}>
        {filtered.map((c,idx)=>(
          <div key={c.id}>
            <div style={{ display:'flex', alignItems:'center', gap:14, padding:'12px 16px' }}>
              <Thumb icon={c.icon} tint={c.tint} size={42} />
              <div style={{ flex:1, display:'flex', flexDirection:'column', gap:2 }}>
                <span style={{ font:`400 16px ${F}`, color:CC.onSurface }}>{c.name}</span>
                <span style={{ font:`400 12px ${F}`, color:CC.onSurfaceVariant }}>{[c.brand,c.cat].filter(Boolean).join(' · ')}</span>
              </div>
              {!c.ref ? <div style={{ display:'flex' }}><IconButton name="pencil-outline" size={20} /><IconButton name="trash-can-outline" size={20} color={CC.error} /></div>
                : <Icon name="chevron-right" size={20} />}
            </div>
            {idx<filtered.length-1 && <Divider inset={72} />}
          </div>
        ))}
      </div>
      <Fab icon="plus" onClick={()=>nav&&nav('addform')} />
    </div>
  );
}

/* ============ PROFILE ============ */
function ProfileScreen() {
  return (
    <div style={{ flex:1, display:'flex', flexDirection:'column', background:CC.background }}>
      <div style={{ display:'flex', flexDirection:'column', alignItems:'center', gap:8, padding:32, background:CC.surface }}>
        <div style={{ width:72, height:72, borderRadius:36, background:CC.primaryContainer, display:'flex', alignItems:'center', justifyContent:'center', font:`400 30px ${F}`, color:CC.onPrimaryContainer }}>A</div>
        <span style={{ font:`500 22px ${F}`, color:CC.onSurface, marginTop:4 }}>Ana Silva</span>
        <span style={{ font:`400 14px ${F}`, color:CC.onSurfaceVariant }}>ana.silva@email.com</span>
      </div>
      <Divider />
      <Row icon="home-account" title="Configurações da residência" desc="Código de convite, membros e endereço" chevron />
      <Divider />
      <Row icon="logout" title="Sair" danger />
    </div>
  );
}
function Row({ icon, title, desc, chevron, danger }) {
  return (
    <div style={{ display:'flex', alignItems:'center', gap:16, padding:'14px 16px', cursor:'pointer' }}>
      <Icon name={icon} size={24} color={danger?CC.error:CC.onSurfaceVariant} />
      <div style={{ flex:1, display:'flex', flexDirection:'column', gap:2 }}>
        <span style={{ font:`400 16px ${F}`, color:danger?CC.error:CC.onSurface }}>{title}</span>
        {desc && <span style={{ font:`400 13px ${F}`, color:CC.onSurfaceVariant }}>{desc}</span>}
      </div>
      {chevron && <Icon name="chevron-right" size={22} />}
    </div>
  );
}

/* ============ RECEIPT SCAN LIST ============ */
function ScanListScreen({ nav }) {
  const statusMap = {
    Parsed:{ l:'Pronto para confirmar', c:CC.secondary },
    Confirmed:{ l:'Confirmado', c:CC.onSurfaceVariant },
    Failed:{ l:'Falhou', c:CC.error },
  };
  return (
    <div style={{ flex:1, display:'flex', flexDirection:'column', position:'relative', background:CC.background }}>
      <div style={{ flex:1, overflowY:'auto', padding:'12px 0 90px' }}>
        {S.scans.map((s,idx)=>{ const st=statusMap[s.status]; const dim=s.status==='Confirmed'; return (
          <div key={s.id}>
            <div onClick={()=> s.status==='Parsed' && nav('confirm', s)} style={{ padding:'12px 16px', opacity:dim?0.6:1, cursor: s.status==='Parsed'?'pointer':'default' }}>
              <div style={{ display:'flex', alignItems:'center', gap:8 }}>
                <div style={{ flex:1, display:'flex', flexDirection:'column', gap:2 }}>
                  <span style={{ font:`400 14px ${F}`, color:CC.onSurface }}>{s.retailer}</span>
                  <span style={{ font:`400 12px ${F}`, color:CC.onSurfaceVariant }}>{s.when}{s.items>0?`  ·  ${s.items} itens`:''}</span>
                </div>
                <span style={{ font:`500 11px ${F}`, color:st.c, textAlign:'right' }}>{st.l}</span>
                {s.status==='Parsed' && <Icon name="chevron-right" size={18} color={CC.secondary} />}
              </div>
              {s.reason && <span style={{ font:`400 12px ${F}`, color:CC.error, marginTop:4, display:'block' }}>{s.reason}</span>}
            </div>
            {idx<S.scans.length-1 && <Divider inset={16} />}
          </div> ); })}
      </div>
      <div style={{ padding:'16px 16px 28px', background:CC.surface, boxShadow:'0 -1px 4px rgba(0,0,0,0.08)' }}>
        <Btn variant="contained" full icon="qrcode-scan" onClick={()=>nav('scancamera')}>Escanear cupom</Btn>
      </div>
    </div>
  );
}

Object.assign(window, { LoginScreen, InventoryScreen, ExpiryScreen, DetailScreen, ShoppingScreen, CatalogScreen, ProfileScreen, ScanListScreen });
