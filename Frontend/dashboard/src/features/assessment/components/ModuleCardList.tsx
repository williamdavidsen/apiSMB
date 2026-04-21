import Box from '@mui/material/Box'
import GppGoodOutlined from '@mui/icons-material/GppGoodOutlined'
import ArticleOutlined from '@mui/icons-material/ArticleOutlined'
import MarkEmailReadOutlined from '@mui/icons-material/MarkEmailReadOutlined'
import LanguageOutlined from '@mui/icons-material/LanguageOutlined'
import { routes } from '../../../shared/constants/routes'
import type { ModuleCardView } from '../model/assessment.mappers'
import { ModuleCard } from './ModuleCard'

type ModuleCardListProps = {
  domain: string
  cards: ModuleCardView[]
}

function iconFor(key: ModuleCardView['key']) {
  const sx = { fontSize: 22, lineHeight: 1 }
  switch (key) {
    case 'ssl-tls':
      return <GppGoodOutlined sx={sx} />
    case 'http-headers':
      return <ArticleOutlined sx={sx} />
    case 'email':
      return <MarkEmailReadOutlined sx={sx} />
    case 'reputation':
      return <LanguageOutlined sx={sx} />
  }
}

export function ModuleCardList({ domain, cards }: ModuleCardListProps) {
  return (
    <Box
      sx={{
        display: 'grid',
        gap: 2,
        gridTemplateColumns: {
          xs: '1fr',
          sm: 'repeat(2, minmax(0, 1fr))',
          lg: 'repeat(4, minmax(0, 1fr))',
        },
      }}
    >
      {cards.map((card) => (
        <ModuleCard
          key={card.key}
          title={card.title}
          icon={iconFor(card.key)}
          grade={card.moduleGrade}
          moduleApiStatus={card.moduleApiStatus}
          scoreFill={card.scoreFill}
          statusLine={card.statusLine}
          facts={card.facts}
          bullet={card.bullet}
          callout={card.callout}
          readMoreTo={`${routes.dashboard}/${encodeURIComponent(domain)}/${card.key}`}
        />
      ))}
    </Box>
  )
}
