import { useState } from 'react'

interface Props {
  rating: number | null
  onChange?: (rating: number) => void
  size?: 'sm' | 'md' | 'lg'
  readonly?: boolean
}

const LABELS = ['', "Didn't enjoy it", 'It was okay', 'Liked it', 'Really liked it', 'It was amazing!']

const PX = { sm: 16, md: 22, lg: 30 }

export default function StarRating({ rating, onChange, size = 'md', readonly = false }: Props) {
  const [hovered, setHovered] = useState<number | null>(null)
  const px = PX[size]
  const active = hovered ?? rating ?? 0

  return (
    <div className="flex flex-col items-center gap-2">
      <div className="flex gap-1.5">
        {[1, 2, 3, 4, 5].map(star => {
          const filled = star <= active
          return (
            <button
              key={star}
              disabled={readonly}
              onMouseEnter={() => !readonly && setHovered(star)}
              onMouseLeave={() => !readonly && setHovered(null)}
              onClick={() => !readonly && onChange?.(star)}
              className="transition-transform active:scale-90"
              style={{
                width: px,
                height: px,
                cursor: readonly ? 'default' : 'pointer',
                transform: !readonly && hovered === star ? 'scale(1.15)' : 'scale(1)',
              }}
            >
              <svg viewBox="0 0 24 24" width={px} height={px}>
                <path
                  d="M11.48 3.499a.562.562 0 011.04 0l2.125 5.111a.563.563 0 00.475.345l5.518.442c.499.04.701.663.321.988l-4.204 3.602a.563.563 0 00-.182.557l1.285 5.385a.562.562 0 01-.84.61l-4.725-2.885a.563.563 0 00-.586 0L6.982 20.54a.562.562 0 01-.84-.61l1.285-5.386a.562.562 0 00-.182-.557l-4.204-3.602a.562.562 0 01.321-.988l5.518-.442a.563.563 0 00.475-.345L11.48 3.5z"
                  fill={filled ? '#C8963E' : 'none'}
                  stroke={filled ? '#C8963E' : '#C4BAA8'}
                  strokeWidth={1.5}
                  strokeLinecap="round"
                  strokeLinejoin="round"
                />
              </svg>
            </button>
          )
        })}
      </div>
      {!readonly && size === 'lg' && (
        <p className="text-sm text-muted h-5 transition-all">
          {hovered ? LABELS[hovered] : rating ? LABELS[rating] : 'Tap to rate'}
        </p>
      )}
    </div>
  )
}
