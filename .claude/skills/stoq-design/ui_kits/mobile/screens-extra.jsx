/* Stoq UI Kit — additional screens: loading skeleton, scan camera, add-item form */
const X = window.STOQ;
const XC = window.STOQ.c;
const XF = window.STOQ.font;

/* ============ LOADING (skeleton) ============ */
function LoadingScreen() {
  const bar = (w, h=14, mt=0) => (
    <div className="sk" style={{ width:w, height:h, marginTop:mt, borderRadius:6 }} />
  );
  return (
    <div style={{ flex:1, background:XC.background, overflowY:'hidden' }}>
      <style>{`@keyframes shim{0%{background-position:-200px 0}100%{background-position:200px 0}}
        .sk{background:linear-gradient(90deg,#EDF1EE 25%,#E2E8E4 37%,#EDF1EE 63%);background-size:400px 100%;animation:shim 1.3s infinite linear;}`}</style>
      {[0,1,2,3,4,5].map(i=>(
        <div key={i} style={{ display:'flex', alignItems:'center', gap:14, padding:'14px 16px' }}>
          <div className="sk" style={{ width:46, height:46, borderRadius:14, flex:'none' }} />
          <div style={{ flex:1 }}>{bar('55%',15)}{bar('38%',12,8)}</div>
        </div>
      ))}
    </div>
  );
}

/* ============ SCAN CAMERA ============ */
function ScanCameraScreen({ mode='receipt', nav }) {
  const isReceipt = mode==='receipt';
  return (
    <div style={{ flex:1, background:'#0E1512', position:'relative', display:'flex', flexDirection:'column' }}>
      {/* faux camera viewport */}
      <div style={{ flex:1, position:'relative', overflow:'hidden',
        background:'radial-gradient(120% 80% at 50% 35%, #25342C 0%, #0E1512 75%)' }}>
        {/* framing reticle */}
        <div style={{ position:'absolute', top:'28%', left:'12%', right:'12%', bottom:'34%' }}>
          {['nw','ne','sw','se'].map(c=>{
            const pos = { nw:{top:0,left:0}, ne:{top:0,right:0}, sw:{bottom:0,left:0}, se:{bottom:0,right:0} }[c];
            const br = { nw:{borderTop:'3px solid #B7E4C7',borderLeft:'3px solid #B7E4C7',borderTopLeftRadius:10},
              ne:{borderTop:'3px solid #B7E4C7',borderRight:'3px solid #B7E4C7',borderTopRightRadius:10},
              sw:{borderBottom:'3px solid #B7E4C7',borderLeft:'3px solid #B7E4C7',borderBottomLeftRadius:10},
              se:{borderBottom:'3px solid #B7E4C7',borderRight:'3px solid #B7E4C7',borderBottomRightRadius:10} }[c];
            return <div key={c} style={{ position:'absolute', width:34, height:34, ...pos, ...br }} />;
          })}
          <div style={{ position:'absolute', left:0, right:0, top:'50%', height:2, background:'#52B788', boxShadow:'0 0 10px #52B788', opacity:.85 }} />
        </div>
        <div style={{ position:'absolute', top:18, left:0, right:0, display:'flex', justifyContent:'center' }}>
          <span style={{ background:'rgba(0,0,0,0.45)', color:'#fff', font:`400 13px ${XF}`, padding:'8px 14px', borderRadius:999, maxWidth:300, textAlign:'center' }}>
            {isReceipt ? 'Aponte para o cupom fiscal. QR codes de NF-e são detectados automaticamente.' : 'Aponte a câmera para o código de barras do produto'}
          </span>
        </div>
      </div>
      {/* control bar */}
      <div style={{ padding:'20px 16px 30px', background:'#0E1512', display:'flex', flexDirection:'column', alignItems:'center', gap:14 }}>
        <button onClick={()=>nav && nav('confirm', window.STOQ.scans[0])} style={{ width:68, height:68, borderRadius:34, background:'#fff', border:'4px solid #52B788', cursor:'pointer',
          display:'flex', alignItems:'center', justifyContent:'center' }}>
          <Icon name={isReceipt?'camera':'barcode-scan'} size={28} color={XC.primary} />
        </button>
        <span style={{ font:`500 13px ${XF}`, color:'#CDE8D9' }}>{isReceipt?'Fotografar cupom':'Escanear código'}</span>
      </div>
    </div>
  );
}

/* ============ ADD ITEM FORM ============ */
function AddItemFormScreen({ nav, snack }) {
  return (
    <div style={{ flex:1, display:'flex', flexDirection:'column', position:'relative', background:XC.background }}>
      <div style={{ flex:1, overflowY:'auto', padding:'18px 16px 100px', display:'flex', flexDirection:'column', gap:16 }}>
        {/* product selector */}
        <div>
          <div style={{ font:`500 13px ${XF}`, color:XC.onSurfaceVariant, textTransform:'uppercase', letterSpacing:0.5, marginBottom:8 }}>Produto</div>
          <div style={{ display:'flex', alignItems:'center', gap:14, padding:'12px 14px', background:XC.surface, borderRadius:14, boxShadow:'0 1px 3px rgba(0,0,0,0.08)' }}>
            <Thumb icon="bottle-soda-outline" tint="tintDairy" size={46} />
            <div style={{ flex:1 }}>
              <div style={{ font:`400 16px ${XF}`, color:XC.onSurface }}>Leite integral</div>
              <div style={{ font:`400 12px ${XF}`, color:XC.onSurfaceVariant }}>Piracanjuba · Laticínios</div>
            </div>
            <Icon name="pencil-outline" size={20} color={XC.primary} />
          </div>
        </div>
        {/* quantity + unit */}
        <div style={{ display:'flex', gap:12 }}>
          <div style={{ width:120 }}><FField label="Quantidade" value="2" /></div>
          <div style={{ flex:1 }}><FField label="Unidade" value="Litros" /></div>
        </div>
        <FField label="Data de validade" value="2026-06-12" hint="AAAA-MM-DD" />
        <FField label="Observações" value="" placeholder="Opcional" />
      </div>
      <div style={{ position:'absolute', left:0, right:0, bottom:0, padding:'14px 16px 28px', background:XC.surface, boxShadow:'0 -1px 5px rgba(0,0,0,0.08)' }}>
        <Btn variant="contained" full icon="check" onClick={()=>{ snack && snack('Adicionado ao estoque.'); nav('inventory'); }}>Adicionar ao estoque</Btn>
      </div>
    </div>
  );
}
function FField({ label, value, placeholder, hint }) {
  return (
    <div style={{ position:'relative' }}>
      <div style={{ position:'absolute', top:-8, left:10, background:XC.background, padding:'0 5px', font:`400 12px ${XF}`, color:XC.onSurfaceVariant, zIndex:1 }}>{label}</div>
      <div style={{ minHeight:52, border:`1px solid ${XC.outline}`, borderRadius:4, display:'flex', alignItems:'center', padding:'0 14px' }}>
        <span style={{ flex:1, font:`400 16px ${XF}`, color: value?XC.onSurface:XC.outline }}>{value||placeholder}</span>
      </div>
      {hint && <div style={{ font:`400 12px ${XF}`, color:XC.onSurfaceVariant, marginTop:5, paddingLeft:4 }}>{hint}</div>}
    </div>
  );
}

Object.assign(window, { LoadingScreen, ScanCameraScreen, AddItemFormScreen });
