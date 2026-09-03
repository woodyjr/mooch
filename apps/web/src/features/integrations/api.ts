export type StravaStatus = {
  isConnected: boolean;
  pendingImportCount: number;
  lastImportedActivityAtUtc?: string;
};

export type ImportedStravaActivity = {
  importedActivityID: string;
  title: string;
  activityType: string;
  distanceMiles: number;
  durationMinutes: number;
  startedAtUtc: string;
  requiresDogAssignment: boolean;
};

export type StravaSyncResult = {
  importedCount: number;
  skippedCount: number;
  pendingImportCount: number;
};

type StravaConnectResponse = {
  authorizationUrl: string;
};

export async function fetchStravaStatus(): Promise<StravaStatus> {
  const response = await fetch('/api/integrations/strava', {
    credentials: 'include'
  });

  if (!response.ok) {
    throw new Error('Could not load your Strava connection status.');
  }

  return response.json() as Promise<StravaStatus>;
}

export async function fetchStravaImports(): Promise<ImportedStravaActivity[]> {
  const response = await fetch('/api/integrations/strava/imports', {
    credentials: 'include'
  });

  if (!response.ok) {
    throw new Error('Could not load your imported Strava activities.');
  }

  return response.json() as Promise<ImportedStravaActivity[]>;
}

export async function beginStravaConnect(): Promise<void> {
  const response = await fetch('/api/integrations/strava/connect', {
    method: 'POST',
    credentials: 'include'
  });

  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || 'Could not start the Strava connection.');
  }

  const payload = (await response.json()) as StravaConnectResponse;
  window.location.assign(payload.authorizationUrl);
}

export async function syncStrava(): Promise<StravaSyncResult> {
  const response = await fetch('/api/integrations/strava/sync', {
    method: 'POST',
    credentials: 'include'
  });

  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || 'Could not sync Strava right now.');
  }

  return response.json() as Promise<StravaSyncResult>;
}
