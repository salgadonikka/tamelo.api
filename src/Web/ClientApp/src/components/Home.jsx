import React, { Component } from 'react';

const ENDPOINT_GROUPS = [
  {
    prefix: '/api/Projects',
    description: 'Create and manage task projects',
    methods: ['GET', 'POST', 'PUT', 'DELETE'],
  },
  {
    prefix: '/api/Tasks',
    description: 'Tasks with circle state, ordering, and archiving',
    methods: ['GET', 'POST', 'PUT', 'DELETE', 'PATCH'],
  },
  {
    prefix: '/api/DayMarkers',
    description: 'Per-day circle state markers on tasks',
    methods: ['PUT', 'DELETE'],
  },
  {
    prefix: '/api/TaskNotes',
    description: 'Notes attached to individual tasks',
    methods: ['GET', 'POST', 'PUT', 'DELETE'],
  },
  {
    prefix: '/api/UserProfiles',
    description: 'User profile settings and preferences',
    methods: ['GET', 'PUT'],
  },
];

const METHOD_COLORS = {
  GET:    { bg: '#e8f5e9', color: '#2e7d32' },
  POST:   { bg: '#e3f2fd', color: '#1565c0' },
  PUT:    { bg: '#fff8e1', color: '#f57f17' },
  PATCH:  { bg: '#f3e5f5', color: '#6a1b9a' },
  DELETE: { bg: '#ffebee', color: '#c62828' },
};

export class Home extends Component {
  static displayName = Home.name;

  constructor(props) {
    super(props);
    this.state = { healthStatus: 'checking' };
  }

  componentDidMount() {
    fetch('/health')
      .then(r => this.setState({ healthStatus: r.ok ? 'healthy' : 'degraded' }))
      .catch(() => this.setState({ healthStatus: 'unreachable' }));
  }

  render() {
    const { healthStatus } = this.state;

    const statusDot = {
      healthy:     { color: '#4caf50', label: 'Healthy' },
      degraded:    { color: '#ff9800', label: 'Degraded' },
      unreachable: { color: '#f44336', label: 'Unreachable' },
      checking:    { color: '#9e9e9e', label: 'Checking…' },
    }[healthStatus];

    return (
      <div style={{ maxWidth: 720, margin: '0 auto', padding: '2rem 1rem', fontFamily: 'inherit' }}>

        {/* Header */}
        <div style={{ marginBottom: '2rem' }}>
          <h1 style={{ fontSize: '2rem', fontWeight: 700, marginBottom: '0.25rem' }}>Tamelo API</h1>
          <p style={{ color: '#666', fontStyle: 'italic', marginBottom: '1rem' }}>
            The Procrastinator's To Do List
          </p>

          {/* Status badge */}
          <span style={{
            display: 'inline-flex', alignItems: 'center', gap: '0.4rem',
            background: '#f5f5f5', border: '1px solid #e0e0e0',
            borderRadius: 20, padding: '0.25rem 0.75rem', fontSize: '0.8rem',
          }}>
            <span style={{
              width: 8, height: 8, borderRadius: '50%',
              background: statusDot.color, display: 'inline-block',
            }} />
            {statusDot.label}
          </span>
        </div>

        {/* Quick links */}
        <div style={{ display: 'flex', gap: '0.75rem', marginBottom: '2.5rem', flexWrap: 'wrap' }}>
          <a href="/api" style={linkCardStyle}>
            <span style={{ fontSize: '1.1rem' }}>📄</span>
            <div>
              <div style={{ fontWeight: 600, fontSize: '0.9rem' }}>API Reference</div>
              <div style={{ fontSize: '0.75rem', color: '#888' }}>Interactive docs via Scalar</div>
            </div>
          </a>
          <a href="/health" style={linkCardStyle}>
            <span style={{ fontSize: '1.1rem' }}>🩺</span>
            <div>
              <div style={{ fontWeight: 600, fontSize: '0.9rem' }}>Health Check</div>
              <div style={{ fontSize: '0.75rem', color: '#888' }}>/health endpoint</div>
            </div>
          </a>
        </div>

        {/* Endpoint groups */}
        <h2 style={{ fontSize: '1rem', fontWeight: 600, color: '#444', marginBottom: '1rem', textTransform: 'uppercase', letterSpacing: '0.05em' }}>
          Endpoints
        </h2>
        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.625rem' }}>
          {ENDPOINT_GROUPS.map(group => (
            <div key={group.prefix} style={endpointCardStyle}>
              <div style={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', gap: '1rem' }}>
                <div>
                  <code style={{ fontWeight: 600, fontSize: '0.875rem', color: '#1a1a1a' }}>{group.prefix}</code>
                  <p style={{ margin: '0.2rem 0 0', fontSize: '0.8rem', color: '#666' }}>{group.description}</p>
                </div>
                <div style={{ display: 'flex', gap: '0.3rem', flexWrap: 'wrap', justifyContent: 'flex-end', flexShrink: 0 }}>
                  {group.methods.map(m => (
                    <span key={m} style={{
                      fontSize: '0.65rem', fontWeight: 700, padding: '0.15rem 0.4rem',
                      borderRadius: 4, fontFamily: 'monospace',
                      background: METHOD_COLORS[m].bg, color: METHOD_COLORS[m].color,
                    }}>
                      {m}
                    </span>
                  ))}
                </div>
              </div>
            </div>
          ))}
        </div>

        {/* Auth note */}
        <p style={{ marginTop: '1.5rem', fontSize: '0.78rem', color: '#999' }}>
          All endpoints require a valid Supabase JWT bearer token.
          Use the <a href="/api" style={{ color: '#0366d6' }}>API Reference</a> to explore and test.
        </p>
      </div>
    );
  }
}

const linkCardStyle = {
  display: 'flex', alignItems: 'center', gap: '0.75rem',
  padding: '0.75rem 1rem', borderRadius: 8,
  border: '1px solid #e0e0e0', textDecoration: 'none', color: 'inherit',
  flex: '1 1 200px', minWidth: 180,
  transition: 'border-color 0.15s',
};

const endpointCardStyle = {
  border: '1px solid #e8e8e8', borderRadius: 8,
  padding: '0.75rem 1rem', background: '#fafafa',
};
