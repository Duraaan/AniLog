import { useState, useEffect, useRef } from 'react';
import { searchAnime } from '../services/api';

export default function SearchBar({ onSelectAnime }) {
  const [query, setQuery] = useState('');
  const [results, setResults] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [open, setOpen] = useState(false);
  const debounceRef = useRef(null);
  const containerRef = useRef(null);

  useEffect(() => {
    if (!query.trim()) {
      setResults([]);
      setOpen(false);
      return;
    }

    clearTimeout(debounceRef.current);
    debounceRef.current = setTimeout(async () => {
      setLoading(true);
      setError('');
      try {
        const res = await searchAnime(query);
        setResults(res.data);
        setOpen(true);
      } catch (e) {
        setError(e.response?.status === 503
          ? 'Servicio temporalmente no disponible. Intenta en unos segundos.'
          : 'Error al buscar. Intenta de nuevo.');
        setResults([]);
      } finally {
        setLoading(false);
      }
    }, 400);

    return () => clearTimeout(debounceRef.current);
  }, [query]);

  useEffect(() => {
    const handleClick = (e) => {
      if (containerRef.current && !containerRef.current.contains(e.target))
        setOpen(false);
    };
    document.addEventListener('mousedown', handleClick);
    return () => document.removeEventListener('mousedown', handleClick);
  }, []);

  const handleSelect = (anime) => {
    onSelectAnime(anime);
    setQuery('');
    setResults([]);
    setOpen(false);
  };

  return (
    <div ref={containerRef} className="relative w-full max-w-xl mx-auto">
      <input
        type="text"
        value={query}
        onChange={(e) => setQuery(e.target.value)}
        placeholder="Buscar anime..."
        className="w-full px-4 py-2 rounded-lg bg-zinc-800 border border-zinc-700 text-white placeholder-zinc-400 focus:outline-none focus:border-purple-500"
      />

      {loading && (
        <p className="absolute mt-1 text-sm text-zinc-400 px-2">Buscando...</p>
      )}

      {error && (
        <p className="absolute mt-1 text-sm text-red-400 px-2">{error}</p>
      )}

      {open && results.length > 0 && (
        <ul className="absolute z-10 w-full mt-1 bg-zinc-800 border border-zinc-700 rounded-lg shadow-lg max-h-80 overflow-y-auto">
          {results.map((anime) => (
            <li
              key={anime.malId}
              className="flex items-center gap-3 px-3 py-2 hover:bg-zinc-700 cursor-pointer"
              onClick={() => handleSelect(anime)}
            >
              <img
                src={anime.images?.jpg?.imageUrl}
                alt={anime.title}
                className="w-10 h-14 object-cover rounded"
              />
              <div className="text-left">
                <p className="text-white text-sm font-medium leading-tight">
                  {anime.titleEnglish || anime.title}
                </p>
                <p className="text-zinc-400 text-xs">{anime.title}</p>
              </div>
            </li>
          ))}
        </ul>
      )}

      {open && !loading && results.length === 0 && query.trim() && (
        <p className="absolute mt-1 text-sm text-zinc-400 px-2">No se encontraron animes.</p>
      )}
    </div>
  );
}
