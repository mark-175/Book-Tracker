export type ReadingStatus = 'to-read' | 'reading' | 'read'

export interface Book {
  id: string
  title: string
  subtitle?: string
  author: string
  pageCount: number
  description: string
  isbn10: string
  isbn13: string
  status: ReadingStatus
  pagesRead: number
  rating: number | null
  notes: string
  startedAt: string | null
  coverHue: number
  coverSat: number
  coverUrl?: string
}

export interface CatalogBook {
  id: string
  title: string
  subtitle?: string
  author: string
  pageCount: number
  description: string
  isbn10: string
  isbn13: string
  coverHue: number
  coverSat: number
  coverUrl?: string
}
