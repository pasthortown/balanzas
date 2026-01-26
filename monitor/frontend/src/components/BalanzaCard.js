import React from 'react';

const styles = {
  card: {
    padding: '20px',
    borderRadius: '8px',
    color: 'white',
    boxShadow: '0 2px 8px rgba(0, 0, 0, 0.1)',
    transition: 'transform 0.2s',
  },
  success: {
    backgroundColor: '#4caf50',
  },
  danger: {
    backgroundColor: '#f44336',
  },
  nombre: {
    fontSize: '18px',
    fontWeight: '600',
    marginBottom: '8px',
  },
  ip: {
    fontSize: '14px',
    opacity: 0.9,
    marginBottom: '8px',
  },
  conexion: {
    fontSize: '12px',
    opacity: 0.8,
    marginBottom: '4px',
  },
  medicion: {
    fontSize: '12px',
    opacity: 0.9,
    fontWeight: '500',
  },
  estado: {
    fontSize: '11px',
    textTransform: 'uppercase',
    marginTop: '10px',
    fontWeight: '600',
  },
  deleteBtn: {
    position: 'absolute',
    top: '8px',
    right: '8px',
    background: 'rgba(255,255,255,0.3)',
    border: 'none',
    borderRadius: '50%',
    width: '24px',
    height: '24px',
    cursor: 'pointer',
    color: 'white',
    fontSize: '14px',
  },
  cardWrapper: {
    position: 'relative',
  },
};

function BalanzaCard({ balanza, onDelete }) {
  const isOk = balanza.estado === 'ok';
  const cardStyle = {
    ...styles.card,
    ...(isOk ? styles.success : styles.danger),
  };

  const formatDate = (dateStr) => {
    if (!dateStr) return 'Nunca';
    const date = new Date(dateStr);
    return date.toLocaleString('es-ES', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
    });
  };

  return (
    <div style={styles.cardWrapper}>
      <div style={cardStyle}>
        <button
          style={styles.deleteBtn}
          onClick={() => onDelete(balanza.id)}
          title="Eliminar"
        >
          ×
        </button>
        <div style={styles.nombre}>{balanza.nombre}</div>
        <div style={styles.ip}>IP: {balanza.ip}</div>
        <div style={styles.conexion}>
          Última conexión OK: {formatDate(balanza.ultimaConexion)}
        </div>
        <div style={styles.medicion}>
          Última medición: {balanza.ultimoPeso != null ? `${balanza.ultimoPeso} kg` : 'Sin datos'} - {formatDate(balanza.ultimaMedicion)}
        </div>
        <div style={styles.estado}>
          Estado: {isOk ? '● Conectado' : '● Desconectado'}
        </div>
      </div>
    </div>
  );
}

export default BalanzaCard;
