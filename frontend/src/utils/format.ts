const FUSO = 'America/Sao_Paulo'

const formatoMoeda = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' })
const formatoNumero = new Intl.NumberFormat('pt-BR')
const formatoDataHora = new Intl.DateTimeFormat('pt-BR', {
  timeZone: FUSO,
  day: '2-digit',
  month: '2-digit',
  year: 'numeric',
  hour: '2-digit',
  minute: '2-digit',
})
const formatoData = new Intl.DateTimeFormat('pt-BR', {
  timeZone: FUSO,
  day: '2-digit',
  month: '2-digit',
  year: 'numeric',
})
const formatoDiaMes = new Intl.DateTimeFormat('pt-BR', { timeZone: 'UTC', day: '2-digit', month: '2-digit' })
const formatoMesAno = new Intl.DateTimeFormat('pt-BR', { timeZone: 'UTC', month: 'short', year: '2-digit' })

export const moeda = (valor: number) => formatoMoeda.format(valor)
export const numero = (valor: number) => formatoNumero.format(valor)

export const dataHora = (iso: string) => formatoDataHora.format(new Date(iso))
export const dataCurta = (iso: string) => formatoData.format(new Date(iso))

// "2026-10-12" -> "12/10" (datas puras são tratadas como UTC para não "voltar um dia").
export const diaMes = (ymd: string) => formatoDiaMes.format(new Date(`${ymd}T00:00:00Z`))

// "2026-10" -> "out. de 26"
export const mesAno = (ym: string) => formatoMesAno.format(new Date(`${ym}-01T00:00:00Z`))

// "2026-10-12" -> "12/10/2026"
export const ymdParaBR = (ymd: string) => {
  const [a, m, d] = ymd.split('-')
  return `${d}/${m}/${a}`
}

// Data de hoje em São Paulo, no formato do <input type="date">.
export function hojeISO(): string {
  return new Intl.DateTimeFormat('en-CA', { timeZone: FUSO }).format(new Date())
}

// Dia (yyyy-MM-dd) de um instante, no calendário de São Paulo.
export function diaISO(iso: string): string {
  return new Intl.DateTimeFormat('en-CA', { timeZone: FUSO }).format(new Date(iso))
}

/* ---------- Valores digitados ---------- */

// Aceita "1.234,56", "1234,56", "1234.56" e "R$ 12,5". Devolve null se não for um valor válido.
export function lerValor(texto: string): number | null {
  const limpo = texto.replace(/[^\d,.-]/g, '').trim()
  if (!limpo) return null

  let normalizado = limpo
  if (limpo.includes(',')) {
    normalizado = limpo.replace(/\./g, '').replace(',', '.')
  } else if ((limpo.match(/\./g) ?? []).length > 1) {
    normalizado = limpo.replace(/\./g, '')
  }

  const valor = Number(normalizado)
  return Number.isFinite(valor) ? Math.round(valor * 100) / 100 : null
}

// 1234.5 -> "1234,50" (para preencher o campo ao editar).
export const valorParaCampo = (valor: number) => valor.toFixed(2).replace('.', ',')

/* ---------- Documentos ---------- */

export const soDigitos = (texto: string) => texto.replace(/\D/g, '')

export function formatarCpf(cpf: string | null | undefined): string {
  const d = soDigitos(cpf ?? '')
  if (d.length !== 11) return cpf ?? ''
  return `${d.slice(0, 3)}.${d.slice(3, 6)}.${d.slice(6, 9)}-${d.slice(9)}`
}

export function formatarTelefone(tel: string | null | undefined): string {
  const d = soDigitos(tel ?? '')
  if (d.length === 11) return `(${d.slice(0, 2)}) ${d.slice(2, 7)}-${d.slice(7)}`
  if (d.length === 10) return `(${d.slice(0, 2)}) ${d.slice(2, 6)}-${d.slice(6)}`
  return tel ?? ''
}

export function formatarCep(cep: string | null | undefined): string {
  const d = soDigitos(cep ?? '')
  return d.length === 8 ? `${d.slice(0, 5)}-${d.slice(5)}` : (cep ?? '')
}

// Máscaras para digitação (campo controlado).
export function mascararCpf(texto: string): string {
  const d = soDigitos(texto).slice(0, 11)
  return d
    .replace(/^(\d{3})(\d)/, '$1.$2')
    .replace(/^(\d{3})\.(\d{3})(\d)/, '$1.$2.$3')
    .replace(/^(\d{3})\.(\d{3})\.(\d{3})(\d)/, '$1.$2.$3-$4')
}

export function mascararTelefone(texto: string): string {
  const d = soDigitos(texto).slice(0, 11)
  if (d.length <= 2) return d
  if (d.length <= 6) return `(${d.slice(0, 2)}) ${d.slice(2)}`
  if (d.length <= 10) return `(${d.slice(0, 2)}) ${d.slice(2, 6)}-${d.slice(6)}`
  return `(${d.slice(0, 2)}) ${d.slice(2, 7)}-${d.slice(7)}`
}

export function mascararCep(texto: string): string {
  const d = soDigitos(texto).slice(0, 8)
  return d.length > 5 ? `${d.slice(0, 5)}-${d.slice(5)}` : d
}

export const rotuloForma = (forma: string) => (forma === 'Cartao' ? 'Cartão' : forma)
