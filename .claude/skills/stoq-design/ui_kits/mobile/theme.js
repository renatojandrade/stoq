/* Stoq UI Kit — design tokens + mock data (plain JS, attached to window) */
window.STOQ = {
  c: {
    primary: '#2D6A4F', onPrimary: '#FFFFFF',
    primaryContainer: '#B7E4C7', onPrimaryContainer: '#0D3321',
    secondary: '#52B788', secondaryContainer: '#D6F0E1', onSecondaryContainer: '#0F3D29',
    background: '#FFFFFF', surface: '#F8F9FA', surface2: '#ECF1ED',
    surfaceVariant: '#DDE5DF', onSurface: '#1A1C1B', onSurfaceVariant: '#404944',
    outline: '#707972', outlineVariant: '#C3C9C4',
    error: '#BA1A1A', errorContainer: '#FFDAD6', onErrorContainer: '#410002',
    warning: '#B26A00', warningContainer: '#FFDFB0', onWarningContainer: '#3A2200',
    // category thumbnail tints (green-family + neutral accents)
    tintDairy: '#E3F0FF', tintProduce: '#E7F6E1', tintPantry: '#FBEFD8',
    tintMeat: '#FBE4E4', tintBev: '#E6EEFB', tintFrozen: '#E2F4F4',
  },
  font: "'Roboto','Helvetica Neue',Arial,system-ui,sans-serif",
  // Mock inventory grouped by product
  inventory: [
    { id:'i1', name:'Leite integral', brand:'Piracanjuba', cat:'Laticínios', icon:'bottle-soda-outline', tint:'tintDairy', qty:1, unit:'Litros', min:2, low:true, batches:2 },
    { id:'i2', name:'Ovos brancos', brand:'Granja Faria', cat:'Laticínios', icon:'egg-outline', tint:'tintDairy', qty:12, unit:'Unidades', min:6, low:false, batches:1 },
    { id:'i3', name:'Arroz branco', brand:'Tio João', cat:'Mercearia', icon:'rice', tint:'tintPantry', qty:5, unit:'Quilogramas', min:2, low:false, batches:1 },
    { id:'i4', name:'Tomate', brand:null, cat:'Hortifrúti', icon:'food-apple-outline', tint:'tintProduce', qty:6, unit:'Unidades', min:0, low:false, batches:1 },
    { id:'i5', name:'Peito de frango', brand:'Sadia', cat:'Carnes', icon:'food-drumstick-outline', tint:'tintMeat', qty:800, unit:'Gramas', min:1000, low:true, batches:2 },
    { id:'i6', name:'Café em pó', brand:'Pilão', cat:'Mercearia', icon:'coffee-outline', tint:'tintPantry', qty:1, unit:'Pacotes', min:1, low:false, batches:1 },
  ],
  expiring: [
    { id:'e1', name:'Iogurte natural', brand:'Nestlé', qty:'4 Unidades', days:0, icon:'cup-outline', tint:'tintDairy' },
    { id:'e2', name:'Peito de frango', brand:'Sadia', qty:'800 Gramas', days:1, icon:'food-drumstick-outline', tint:'tintMeat' },
    { id:'e3', name:'Leite integral', brand:'Piracanjuba', qty:'1 Litro', days:3, icon:'bottle-soda-outline', tint:'tintDairy' },
    { id:'e4', name:'Pão de forma', brand:'Pullman', qty:'1 Pacote', days:5, icon:'bread-slice-outline', tint:'tintPantry' },
  ],
  shopping: [
    { id:'s1', name:'Banana prata', qty:'1 Dúzia', checked:false, source:'Manual', icon:'food-apple-outline', tint:'tintProduce' },
    { id:'s2', name:'Leite integral', qty:'6 Litros', checked:false, source:'Auto', icon:'bottle-soda-outline', tint:'tintDairy' },
    { id:'s3', name:'Detergente', qty:'2 Unidades', checked:false, source:'Manual', icon:'spray-bottle', tint:'tintBev' },
    { id:'s4', name:'Peito de frango', qty:'1 Quilograma', checked:true, source:'Auto', icon:'food-drumstick-outline', tint:'tintMeat' },
    { id:'s5', name:'Sabão em pó', qty:'1 Pacote', checked:true, source:'Manual', icon:'washing-machine', tint:'tintBev' },
  ],
  catalog: [
    { id:'c1', name:'Leite integral 1L', brand:'Piracanjuba', cat:'Laticínios', ref:true, icon:'bottle-soda-outline', tint:'tintDairy' },
    { id:'c2', name:'Arroz branco 5kg', brand:'Tio João', cat:'Mercearia', ref:true, icon:'rice', tint:'tintPantry' },
    { id:'c3', name:'Refrigerante cola 2L', brand:'Coca-Cola', cat:'Bebidas', ref:true, icon:'bottle-soda', tint:'tintBev' },
    { id:'c4', name:'Marmita caseira', brand:null, cat:'Minha casa', ref:false, icon:'home-outline', tint:'tintProduce' },
  ],
  scans: [
    { id:'r1', retailer:'Supermercado Pão de Açúcar', when:'Hoje, 14:32', items:14, status:'Parsed' },
    { id:'r2', retailer:'Atacadão', when:'Ontem, 19:05', items:23, status:'Confirmed' },
    { id:'r3', retailer:'Estabelecimento desconhecido', when:'12 mai, 10:11', items:0, status:'Failed', reason:'Não foi possível ler o cupom.' },
  ],
  receiptItems: [
    { id:'p1', name:'LEITE INT PIRACANJUBA 1L', qty:'6', unit:'Unidades', expiry:'', matched:true },
    { id:'p2', name:'ARROZ TIO JOAO 5KG', qty:'1', unit:'Pacotes', expiry:'', matched:true },
    { id:'p3', name:'BANANA PRATA KG', qty:'1.2', unit:'Quilogramas', expiry:'', matched:false },
    { id:'p4', name:'CAFE PILAO 500G', qty:'2', unit:'Pacotes', expiry:'', matched:true },
  ],
};
