import AnimeCard from './AnimeCard';

const FILTERS = [
  { label: 'Todos', value: '' },
  { label: 'Viendo', value: 'Watching' },
  { label: 'Completado', value: 'Completed' },
  { label: 'Abandonado', value: 'Dropped' },
  { label: 'Planeo ver', value: 'PlanToWatch' },
];

export default function AnimeList({ animes, loading, activeFilter, onFilterChange, onRefresh, page, totalPages, onPageChange }) {
  return (
    <div>
      <div className="flex gap-2 flex-wrap mb-6">
        {FILTERS.map((f) => (
          <button
            key={f.value}
            onClick={() => onFilterChange(f.value)}
            className={`px-4 py-1.5 rounded-full text-sm transition ${
              activeFilter === f.value
                ? 'bg-purple-600 text-white'
                : 'bg-zinc-800 text-zinc-400 hover:bg-zinc-700'
            }`}
          >
            {f.label}
          </button>
        ))}
      </div>

      {loading ? (
        <p className="text-zinc-400 text-center py-12">Cargando...</p>
      ) : animes.length === 0 ? (
        <p className="text-zinc-500 text-center py-12">
          No tenés animes en tu lista. ¡Buscá uno para empezar!
        </p>
      ) : (
        <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-4">
          {animes.map((anime) => (
            <AnimeCard key={anime.id} anime={anime} onRefresh={onRefresh} />
          ))}
        </div>
      )}

      {totalPages > 1 && (
        <div className="flex justify-center items-center gap-3 mt-8">
          <button
            onClick={() => onPageChange(page - 1)}
            disabled={page === 1}
            className="px-4 py-1.5 rounded-lg bg-zinc-800 text-zinc-400 hover:bg-zinc-700 disabled:opacity-30 disabled:cursor-not-allowed transition"
          >
            ← Anterior
          </button>
          <span className="text-zinc-400 text-sm">
            Página {page} de {totalPages}
          </span>
          <button
            onClick={() => onPageChange(page + 1)}
            disabled={page === totalPages}
            className="px-4 py-1.5 rounded-lg bg-zinc-800 text-zinc-400 hover:bg-zinc-700 disabled:opacity-30 disabled:cursor-not-allowed transition"
          >
            Siguiente →
          </button>
        </div>
      )}
    </div>
  );
}
