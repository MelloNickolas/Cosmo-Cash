import { Button, InlineNotification, Modal } from '@carbon/react'
import type { ReactNode } from 'react'

export function PaginaCabecalho({
  titulo,
  subtitulo,
  children,
}: {
  titulo: string
  subtitulo?: string
  children?: ReactNode
}) {
  return (
    <header className="pagina-cabecalho">
      <div>
        <h1>{titulo}</h1>
        {subtitulo && <p>{subtitulo}</p>}
      </div>
      {children && <div className="acoes">{children}</div>}
    </header>
  )
}

export function Vazio({ titulo, texto, children }: { titulo: string; texto?: string; children?: ReactNode }) {
  return (
    <div className="vazio">
      <h3>{titulo}</h3>
      {texto && <p>{texto}</p>}
      {children}
    </div>
  )
}

export function ErroCarregar({ mensagem, aoTentar }: { mensagem: string; aoTentar?: () => void }) {
  return (
    <div>
      <InlineNotification
        kind="error"
        lowContrast
        hideCloseButton
        title="Não foi possível carregar"
        subtitle={mensagem}
      />
      {aoTentar && (
        <Button kind="tertiary" size="sm" onClick={aoTentar}>
          Tentar de novo
        </Button>
      )}
    </div>
  )
}

export function ConfirmarModal({
  aberto,
  titulo,
  texto,
  rotuloConfirmar,
  perigo = false,
  carregando = false,
  aoConfirmar,
  aoCancelar,
}: {
  aberto: boolean
  titulo: string
  texto: ReactNode
  rotuloConfirmar: string
  perigo?: boolean
  carregando?: boolean
  aoConfirmar: () => void
  aoCancelar: () => void
}) {
  return (
    <Modal
      open={aberto}
      size="xs"
      danger={perigo}
      modalHeading={titulo}
      primaryButtonText={carregando ? 'Aguarde…' : rotuloConfirmar}
      secondaryButtonText="Voltar"
      primaryButtonDisabled={carregando}
      onRequestSubmit={aoConfirmar}
      onRequestClose={aoCancelar}
      onSecondarySubmit={aoCancelar}
    >
      <p>{texto}</p>
    </Modal>
  )
}
