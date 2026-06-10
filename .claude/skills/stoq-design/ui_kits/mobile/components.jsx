/* Stoq UI Kit — reusable primitives. Exposes components on window. */
const { useState } = React;
const C = window.STOQ.c;
const FONT = window.STOQ.font;

/* ---- Icon (Material Community Icons via @mdi/font) ---- */
function Icon({ name, size = 22, color = C.onSurfaceVariant, style }) {
  return <span className={`mdi mdi-${name}`} style={{ fontSize: size, lineHeight: 1, color, ...style }} />;
}

/* ---- Status bar (faux iOS) ---- */
function StatusBar() {
  return (
    <div style={{ height: 44, display:'flex', alignItems:'center', justifyContent:'space-between',
      padding:'0 22px', font:`500 15px ${FONT}`, color: C.onSurface, flex:'none' }}>
      <span>9:41</span>
      <div style={{ display:'flex', gap:6, alignItems:'center' }}>
        <Icon name="signal" size={16} color={C.onSurface} />
        <Icon name="wifi" size={16} color={C.onSurface} />
        <Icon name="battery" size={18} color={C.onSurface} />
      </div>
    </div>
  );
}

/* ---- App header (top bar) ---- */
function AppHeader({ title, onBack, right }) {
  return (
    <div style={{ height:56, display:'flex', alignItems:'center', gap:6, padding:'0 6px 0 4px',
      background: C.background, borderBottom:`1px solid ${C.outlineVariant}`, flex:'none' }}>
      {onBack
        ? <button onClick={onBack} style={iconBtn}><Icon name="arrow-left" size={24} color={C.onSurface} /></button>
        : <div style={{ width: 12 }} />}
      <span style={{ font:`500 20px ${FONT}`, color: C.onSurface, flex:1 }}>{title}</span>
      {right}
    </div>
  );
}

/* ---- Button (MD3 variants) ---- */
function Btn({ variant='contained', icon, children, onClick, disabled, full, color }) {
  const base = { display:'inline-flex', alignItems:'center', justifyContent:'center', gap:8,
    height:42, padding: variant==='text' ? '0 12px' : '0 22px', borderRadius:21,
    font:`500 14px ${FONT}`, border:'none', cursor: disabled?'default':'pointer',
    width: full?'100%':'auto', letterSpacing:0.1, transition:'filter .12s' };
  const variants = {
    contained: { background: disabled?'#E3E5E4':C.primary, color: disabled?'#A0A8A3':'#fff' },
    tonal: { background: C.primaryContainer, color: C.onPrimaryContainer },
    outlined: { background:'transparent', color: color||C.primary, border:`1px solid ${C.outlineVariant}` },
    text: { background:'transparent', color: color||C.primary },
  };
  return (
    <button onClick={disabled?undefined:onClick} style={{ ...base, ...variants[variant] }}>
      {icon && <Icon name={icon} size={18} color={variants[variant].color} />}
      {children}
    </button>
  );
}

/* ---- Icon button ---- */
const iconBtn = { width:40, height:40, borderRadius:20, border:'none', background:'transparent',
  display:'inline-flex', alignItems:'center', justifyContent:'center', cursor:'pointer', padding:0 };
function IconButton({ name, color=C.onSurfaceVariant, size=22, onClick }) {
  return <button onClick={onClick} style={iconBtn}><Icon name={name} size={size} color={color} /></button>;
}

/* ---- Chip (pill) ---- */
function Chip({ children, bg=C.surfaceVariant, fg=C.onSurfaceVariant, outline }) {
  return (
    <span style={{ display:'inline-flex', alignItems:'center', gap:5, height:24, padding:'0 11px',
      borderRadius:999, font:`500 11px ${FONT}`, background: outline?'transparent':bg, color: fg,
      border: outline?`1px solid ${fg}`:'none', alignSelf:'flex-start' }}>{children}</span>
  );
}

/* ---- Category thumbnail (richer than plain grey circle) ---- */
function Thumb({ icon, tint, size=46 }) {
  return (
    <div style={{ width:size, height:size, borderRadius:14, background: C[tint]||C.surfaceVariant,
      display:'flex', alignItems:'center', justifyContent:'center', flex:'none' }}>
      <Icon name={icon} size={size*0.5} color={C.onSurface} style={{ opacity:.78 }} />
    </div>
  );
}

/* ---- Search bar ---- */
function SearchBar({ placeholder, value='', onChange }) {
  return (
    <div style={{ display:'flex', alignItems:'center', gap:10, height:48, padding:'0 14px', margin:'12px 16px',
      background: C.surface2, borderRadius:28 }}>
      <Icon name="magnify" size={22} color={C.onSurfaceVariant} />
      <input value={value} onChange={e=>onChange&&onChange(e.target.value)} placeholder={placeholder}
        style={{ flex:1, border:'none', outline:'none', background:'transparent', font:`400 15px ${FONT}`, color:C.onSurface }} />
    </div>
  );
}

/* ---- Banner (tappable) ---- */
function Banner({ icon, children, tone='neutral', onClick }) {
  const tones = {
    alert: { bg:C.errorContainer, fg:C.onErrorContainer },
    neutral: { bg:C.surfaceVariant, fg:C.onSurfaceVariant },
  };
  const tn = tones[tone];
  return (
    <div onClick={onClick} style={{ display:'flex', alignItems:'center', gap:10, padding:'13px 16px',
      background: tn.bg, color: tn.fg, cursor:'pointer', flex:'none' }}>
      <Icon name={icon} size={20} color={tn.fg} />
      <span style={{ flex:1, font:`500 14px ${FONT}` }}>{children}</span>
      <Icon name="chevron-right" size={20} color={tn.fg} />
    </div>
  );
}

/* ---- FAB ---- */
function Fab({ icon, label, onClick, bottom=20 }) {
  return (
    <button onClick={onClick} style={{ position:'absolute', right:16, bottom, height:56,
      borderRadius:16, padding: label?'0 20px':0, width: label?'auto':56, gap:8,
      background:C.primaryContainer, color:C.onPrimaryContainer, border:'none', cursor:'pointer',
      display:'flex', alignItems:'center', justifyContent:'center', font:`500 14px ${FONT}`,
      boxShadow:'0 3px 6px rgba(0,0,0,0.22)' }}>
      <Icon name={icon} size={24} color={C.onPrimaryContainer} />{label}
    </button>
  );
}

/* ---- Empty state ---- */
function EmptyState({ icon, title, subtitle, action }) {
  return (
    <div style={{ flex:1, display:'flex', flexDirection:'column', alignItems:'center', justifyContent:'center',
      gap:12, padding:'0 40px', textAlign:'center' }}>
      <Icon name={icon} size={52} color={C.onSurfaceVariant} style={{ opacity:.5 }} />
      <span style={{ font:`500 16px ${FONT}`, color:C.onSurface }}>{title}</span>
      <span style={{ font:`400 14px ${FONT}`, color:C.onSurfaceVariant, maxWidth:280 }}>{subtitle}</span>
      <div style={{ marginTop:4, display:'flex', flexDirection:'column', gap:10, width:'100%', alignItems:'center' }}>{action}</div>
    </div>
  );
}

/* ---- Divider ---- */
function Divider({ inset=0 }) {
  return <div style={{ height:1, background:C.outlineVariant, marginLeft:inset }} />;
}

/* ---- Snackbar ---- */
function Snackbar({ text, action, visible }) {
  if (!visible) return null;
  return (
    <div style={{ position:'absolute', left:16, right:16, bottom:96, background:'#2E312F', color:'#ECF1ED',
      borderRadius:8, padding:'12px 16px', display:'flex', alignItems:'center', gap:12, font:`400 14px ${FONT}`,
      boxShadow:'0 3px 8px rgba(0,0,0,0.3)', zIndex:30 }}>
      <span style={{ flex:1 }}>{text}</span>
      {action && <span style={{ color:C.secondary, font:`500 14px ${FONT}` }}>{action}</span>}
    </div>
  );
}

/* ---- Bottom tab bar (5 tabs + center scan) ---- */
const TABS = [
  { key:'inventory', label:'Estoque', icon:'fridge' },
  { key:'shopping', label:'Lista', icon:'cart' },
  { key:'__scan', label:'', icon:'barcode-scan' },
  { key:'catalog', label:'Catálogo', icon:'package-variant' },
  { key:'profile', label:'Perfil', icon:'account-circle' },
];
function TabBar({ active, onSelect, onScan }) {
  return (
    <div style={{ display:'flex', height:62, background:C.surface, borderTop:`1px solid ${C.outlineVariant}`,
      padding:'0 4px', flex:'none', paddingBottom:6 }}>
      {TABS.map(t => {
        if (t.key === '__scan') {
          return (
            <div key="scan" style={{ flex:1, display:'flex', alignItems:'center', justifyContent:'center' }}>
              <button onClick={onScan} aria-label="Escanear" style={{ width:56, height:56, borderRadius:28,
                background:C.primary, border:'none', cursor:'pointer', marginTop:-16, display:'flex',
                alignItems:'center', justifyContent:'center', boxShadow:'0 2px 5px rgba(0,0,0,0.3)' }}>
                <Icon name="barcode-scan" size={26} color="#fff" />
              </button>
            </div>
          );
        }
        const on = active === t.key;
        const icon = on ? t.icon : t.icon + '-outline';
        return (
          <button key={t.key} onClick={()=>onSelect(t.key)} style={{ flex:1, border:'none', background:'transparent',
            cursor:'pointer', display:'flex', flexDirection:'column', alignItems:'center', justifyContent:'center', gap:3, paddingTop:6 }}>
            <Icon name={icon} size={24} color={on?C.primary:C.onSurfaceVariant} />
            <span style={{ font:`500 11px ${FONT}`, color:on?C.primary:C.onSurfaceVariant }}>{t.label}</span>
          </button>
        );
      })}
    </div>
  );
}

Object.assign(window, { Icon, IconButton, StatusBar, AppHeader, Btn, Chip, Thumb, SearchBar, Banner, Fab, EmptyState, Divider, Snackbar, TabBar, iconBtn });
