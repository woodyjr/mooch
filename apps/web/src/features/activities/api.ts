export type CreateManualActivityInput = {
  dogIds: string[];
  title: string;
  notes?: string;
  distanceMiles: number;
  durationMinutes: number;
  startedAtUtc: string;
};

export type ManualActivityResponse = {
  activityID: string;
  dogIDs: string[];
  walkerID: string;
  title: string;
  notes?: string | null;
  distanceMiles: number;
  durationMinutes: number;
  startedAtUtc: string;
  source: string;
};

export async function createManualActivity(input: CreateManualActivityInput): Promise<ManualActivityResponse> {
  const response = await fetch('/api/activities/manual', {
    method: 'POST',
    credentials: 'include',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      dogIDs: input.dogIds,
      title: input.title.trim(),
      notes: input.notes?.trim() || null,
      distanceMiles: input.distanceMiles,
      durationMinutes: input.durationMinutes,
      startedAtUtc: input.startedAtUtc
    })
  });

  if (!response.ok) {
    const message = await response.text();
    console.log(response);
    throw new Error(message || 'Could not save that activity right now.');
  }

  return response.json() as Promise<ManualActivityResponse>;
}
