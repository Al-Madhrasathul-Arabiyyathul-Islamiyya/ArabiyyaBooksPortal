# Schema Addendum: Unicode & Timezone

## 1. Multi-Script Text Support

Book titles and other text fields may contain English, Arabic, or Dhivehi (Thaana script).

### Database
- **Already handled:** All text columns use `NVARCHAR` which supports Unicode
- No schema changes required

### Frontend
Add CSS for proper RTL text rendering:

```css
/* Auto-detect text direction */
.text-auto-dir {
  unicode-bidi: plaintext;
  text-align: start;
}
```

Apply to fields that may contain Arabic/Dhivehi text (titles, names, notes).

### Collation (Optional)
If sorting by title becomes problematic with mixed scripts, consider:
```sql
-- Check current database collation
SELECT DATABASEPROPERTYEX('BooksPortal', 'Collation')

-- For mixed-language sorting, this collation works reasonably well
-- (Set at database creation or per-column if needed)
-- Latin1_General_100_CI_AS_SC_UTF8
```

---

## 2. Timezone Handling

**Local timezone:** UTC+5 (Maldives, no daylight saving)

### Strategy
| Layer | Format |
|-------|--------|
| Database | UTC |
| API | ISO 8601 with Z suffix |
| Display | UTC+5 |

### Backend (.NET)

```csharp
// Always save UTC
entity.CreatedAt = DateTime.UtcNow;
entity.IssuedAt = DateTime.UtcNow;

// appsettings.json - for reference if needed
{
  "AppSettings": {
    "Timezone": "Indian/Maldives"  // or "UTC+5"
  }
}
```

### Frontend (dayjs)

Install timezone plugins:
```bash
npm install dayjs
```

Configure in plugin:
```typescript
// plugins/dayjs.ts
import dayjs from 'dayjs'
import utc from 'dayjs/plugin/utc'
import timezone from 'dayjs/plugin/timezone'

dayjs.extend(utc)
dayjs.extend(timezone)

const LOCAL_TZ = 'Indian/Maldives' // UTC+5

export function formatLocal(utcDate: string, format = 'DD/MM/YYYY hh:mm A') {
  return dayjs.utc(utcDate).tz(LOCAL_TZ).format(format)
}

export function formatDate(utcDate: string) {
  return dayjs.utc(utcDate).tz(LOCAL_TZ).format('DD/MM/YYYY')
}

export function formatTime(utcDate: string) {
  return dayjs.utc(utcDate).tz(LOCAL_TZ).format('hh:mm A')
}
```

Usage in components:
```vue
<template>
  <span>{{ formatLocal(slip.issuedAt) }}</span>
</template>

<script setup>
import { formatLocal } from '~/plugins/dayjs'
</script>
```

### Print Slips
Ensure printed slips show local time:
```typescript
// In PDF/print generation, convert before display
const localIssuedAt = formatLocal(slip.issuedAt, 'DD/MM/YYYY hh:mm A')
```
