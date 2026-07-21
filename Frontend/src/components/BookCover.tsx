interface CoverProps {
  title: string
  author: string
  coverHue: number
  coverSat: number
}

interface Props {
  book: CoverProps
  size?: 'xs' | 'sm' | 'md' | 'lg' | 'xl'
}

const SIZES = {
  xs:  { outer: 'w-9 h-[52px]',   title: '7px',  author: '5px'  },
  sm:  { outer: 'w-12 h-[70px]',  title: '8px',  author: '6px'  },
  md:  { outer: 'w-20 h-28',      title: '10px', author: '7px'  },
  lg:  { outer: 'w-28 h-40',      title: '11px', author: '8px'  },
  xl:  { outer: 'w-36 h-52',      title: '12px', author: '9px'  },
}

export default function BookCover({ book, size = 'md' }: Props) {
  const s = SIZES[size]
  const h = book.coverHue
  const sat = book.coverSat

  return (
    <div
      className={`${s.outer} rounded flex-shrink-0 relative overflow-hidden`}
      style={{
        background: `linear-gradient(155deg, hsl(${h},${sat}%,20%) 0%, hsl(${h + 28},${sat - 4}%,33%) 55%, hsl(${h + 10},${sat}%,26%) 100%)`,
        boxShadow: '2px 3px 10px rgba(44,26,14,0.35), inset 1px 0 0 rgba(255,255,255,0.06)',
      }}
    >
      {/* Spine shadow */}
      <div
        className="absolute left-0 top-0 bottom-0 w-[3px]"
        style={{ background: `hsl(${h},${sat}%,12%)` }}
      />
      {/* Top edge highlight */}
      <div className="absolute top-0 left-0 right-0 h-px" style={{ background: 'rgba(255,255,255,0.18)' }} />
      {/* Diagonal gloss */}
      <div
        className="absolute inset-0"
        style={{ background: 'linear-gradient(135deg, rgba(255,255,255,0.08) 0%, transparent 50%)' }}
      />

      <div className="absolute inset-0 flex flex-col justify-end p-1.5 pl-2.5">
        <p
          className="font-serif text-white leading-tight font-medium"
          style={{
            fontSize: s.title,
            display: '-webkit-box',
            WebkitBoxOrient: 'vertical',
            WebkitLineClamp: 4,
            overflow: 'hidden',
            opacity: 0.92,
          }}
        >
          {book.title}
        </p>
        <p
          className="text-white mt-0.5 leading-tight"
          style={{ fontSize: s.author, opacity: 0.5, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}
        >
          {book.author.split(' ').slice(-1)[0]}
        </p>
      </div>
    </div>
  )
}
