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
  coverHue: number
  coverSat: number
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
}

export const INITIAL_BOOKS: Book[] = [
  {
    id: '1',
    title: 'The Name of the Wind',
    subtitle: 'The Kingkiller Chronicle: Day One',
    author: 'Patrick Rothfuss',
    pageCount: 662,
    description:
      "Told in Kvothe's own voice, this is the tale of the magically gifted young man who grows to be one of the most notorious wizards his world has ever seen. A high-fantasy epic told with rare intimacy and unforgettable prose.",
    isbn10: '0756404746',
    isbn13: '9780756404741',
    status: 'reading',
    pagesRead: 320,
    rating: null,
    notes:
      "Really enjoying Kvothe's voice. The sympathy magic system is fascinating — the way it treats energy exchange like physics is unlike any magic system I've encountered.",
    coverHue: 220,
    coverSat: 52,
  },
  {
    id: '2',
    title: 'Project Hail Mary',
    author: 'Andy Weir',
    pageCount: 476,
    description:
      "Ryland Grace is the sole survivor on a desperate, last-chance mission — and if he fails, humanity and the Earth itself will perish. Except right now he doesn't know that. He can't even remember his own name.",
    isbn10: '0593135202',
    isbn13: '9780593135204',
    status: 'read',
    pagesRead: 476,
    rating: 5,
    notes:
      "One of the best sci-fi novels I've read in years. Rocky is an extraordinary character — the chapters where they first communicate are some of the most creative writing I've encountered.",
    coverHue: 185,
    coverSat: 58,
  },
  {
    id: '3',
    title: 'Dune',
    author: 'Frank Herbert',
    pageCount: 688,
    description:
      'Set on the desert planet Arrakis, Dune is the story of Paul Atreides, heir to a noble family tasked with ruling an inhospitable world where the only thing of value is the spice melange — a substance capable of extending life and enhancing consciousness.',
    isbn10: '0441013597',
    isbn13: '9780441013593',
    status: 'to-read',
    pagesRead: 0,
    rating: null,
    notes: '',
    coverHue: 32,
    coverSat: 65,
  },
  {
    id: '4',
    title: 'The Midnight Library',
    author: 'Matt Haig',
    pageCount: 304,
    description:
      'Between life and death there is a library, and within that library, the shelves go on forever. Every book provides a chance to try another life you could have lived. Which would you choose?',
    isbn10: '0525559477',
    isbn13: '9780525559474',
    status: 'read',
    pagesRead: 304,
    rating: 4,
    notes: 'Beautiful and thought-provoking. Made me reflect on regrets and the infinite possibilities of a life.',
    coverHue: 265,
    coverSat: 48,
  },
  {
    id: '5',
    title: 'Piranesi',
    author: 'Susanna Clarke',
    pageCount: 272,
    description:
      "Piranesi's house is no ordinary building: its halls are infinite, its corridors full of statues, its staircases rising into the clouds. But there is a mystery at its heart. Who else lives in the House, and what do they want?",
    isbn10: '1635575567',
    isbn13: '9781635575569',
    status: 'to-read',
    pagesRead: 0,
    rating: null,
    notes: '',
    coverHue: 155,
    coverSat: 42,
  },
  {
    id: '6',
    title: 'Lessons in Chemistry',
    author: 'Bonnie Garmus',
    pageCount: 320,
    description:
      "Chemist Elizabeth Zott is not your average woman. In fact, Elizabeth Zott would be the first to point out that there is no such thing as an average woman. But it's the early 1960s and her all-male team takes a very unscientific view of equality.",
    isbn10: '0385547943',
    isbn13: '9780385547949',
    status: 'reading',
    pagesRead: 145,
    rating: null,
    notes: 'Witty and empowering. Elizabeth Zott is a genuinely original protagonist.',
    coverHue: 8,
    coverSat: 58,
  },
  {
    id: '7',
    title: 'The Seven Husbands of Evelyn Hugo',
    author: 'Taylor Jenkins Reid',
    pageCount: 389,
    description:
      'Aging and reclusive Hollywood movie icon Evelyn Hugo is finally ready to tell the truth about her glamorous and scandalous life. But she is only willing to speak to unknown magazine reporter Monique Grant.',
    isbn10: '1501161938',
    isbn13: '9781501161933',
    status: 'to-read',
    pagesRead: 0,
    rating: null,
    notes: '',
    coverHue: 330,
    coverSat: 48,
  },
]

export const SEARCH_CATALOG: CatalogBook[] = [
  {
    id: 'cat-1',
    title: 'Tomorrow, and Tomorrow, and Tomorrow',
    author: 'Gabrielle Zevin',
    pageCount: 480,
    description:
      'Two friends come together as creative partners in the world of video game design, exploring love, loss, identity, and the meaning of play.',
    isbn10: '0593321200',
    isbn13: '9780593321201',
    coverHue: 290,
    coverSat: 42,
  },
  {
    id: 'cat-2',
    title: 'A Court of Thorns and Roses',
    author: 'Sarah J. Maas',
    pageCount: 419,
    description: 'A young huntress is taken to a magical land after killing a wolf in the woods.',
    isbn10: '1619635178',
    isbn13: '9781619635173',
    coverHue: 340,
    coverSat: 52,
  },
  {
    id: 'cat-3',
    title: 'Fourth Wing',
    author: 'Rebecca Yarros',
    pageCount: 528,
    description:
      'Violet Sorrengail enters the most dangerous war college in the empire, where riders and their dragons reign supreme.',
    isbn10: '1649374046',
    isbn13: '9781649374042',
    coverHue: 0,
    coverSat: 58,
  },
  {
    id: 'cat-4',
    title: 'Atomic Habits',
    author: 'James Clear',
    pageCount: 320,
    description:
      'An easy and proven way to build good habits and break bad ones — a practical guide to how tiny changes can transform your life.',
    isbn10: '0735211299',
    isbn13: '9780735211292',
    coverHue: 44,
    coverSat: 68,
  },
  {
    id: 'cat-5',
    title: 'The Song of Achilles',
    author: 'Madeline Miller',
    pageCount: 352,
    description:
      'Greece in the age of heroes. Prince Patroclus and the warrior Achilles forge a bond that will alter the course of history.',
    isbn10: '0062060621',
    isbn13: '9780062060624',
    coverHue: 200,
    coverSat: 48,
  },
  {
    id: 'cat-6',
    title: 'Babel',
    subtitle: 'Or the Necessity of Violence',
    author: 'R.F. Kuang',
    pageCount: 560,
    description:
      "A dark academia novel set in 1830s Oxford, exploring the magic of translation, colonialism, and the cost of complicity.",
    isbn10: '0063021420',
    isbn13: '9780063021426',
    coverHue: 170,
    coverSat: 44,
  },
]
