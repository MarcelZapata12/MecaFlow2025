/* Header flexible reutilizable en tarjetas (Empleados/Clientes) */
.card - header - flex{
    display: flex;
    align - items: flex - start;
    gap: .75rem;
    margin - bottom: .25rem;
}

/* Título que se corta con ellipsis dentro de un contenedor flex */
.card - title - ellipsis{
    margin: 0;
    display: flex;
    align - items: center;
    gap: .5rem;
    flex: 1 1 auto;   /* ocupa el espacio disponible */
    min - width: 0;     /* necesario para que el ellipsis funcione en flex */
    color:#66ccff;
    font - weight: 700;
}
.card - title - ellipsis.name{
    display: inline - block;
    min - width: 0;
    max - width: 100 %;
    white - space: nowrap;
    overflow: hidden;
    text - overflow: ellipsis;
}

/* Grupo de acciones a la derecha (ya NO absolute) */
.card - actions{
    display: flex;
    gap: .5rem;
    flex - shrink: 0;
}
.card - actions.icon - btn{
    background: none; border: 0; padding: 0; line - height: 1; font - size: 1.1rem;
}

/* Opcional: en móviles, permitir 2 líneas para el nombre */
@media(max - width: 420px) {
  .card - title - ellipsis.name{
        white - space: normal;
        display: -webkit - box;
        -webkit - line - clamp: 2;
        -webkit - box - orient: vertical;
    }
}
