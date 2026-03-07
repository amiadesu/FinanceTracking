import * as v from 'valibot';

export const inviteSchema = v.object({
    targetUserIdentifier: v.pipe(v.string(), v.minLength(1, 'Email or Username is required')),
    note: v.optional(v.pipe(v.string(), v.maxLength(500, 'Note must be less than 500 characters')))
});

export const categorySchema = v.object({
    name: v.pipe(
        v.string(),
        v.nonEmpty('Name is required'),
        v.maxLength(100, 'Name must be less than 100 characters')
    ),
    colorHex: v.pipe(
        v.string(),
        v.regex(/^#([0-9A-Fa-f]{6})$/, 'Color must be a valid hex code')
    )
});

export const budgetGoalSchema = v.pipe(
    v.object({
        targetAmount: v.pipe(
            v.number(),
            v.minValue(1, 'Target amount must be greater than 0')
        ),
        startDate: v.pipe(
            v.string(),
            v.nonEmpty('Start date is required'),
            v.isoDate('Start date must be a valid date')
        ),
        endDate: v.pipe(
            v.string(),
            v.nonEmpty('End date is required'),
            v.isoDate('End date must be a valid date')
        )
    }),
    v.check(
        (input) => input.startDate < input.endDate, 
        'Start date must be before end date'
    )
);

export const memberRoleSchema = v.object({
  role: v.object({
    label: v.string(),
    value: v.number()
  })
});

export const groupEditSchema = v.object({
  name: v.pipe(
    v.string(), 
    v.minLength(1, 'Group name is required'),
    v.maxLength(100, 'Group name must be less than 100 characters')
  )
});

export const groupResetSchema = v.object({
  selectedOptions: v.pipe(
    v.array(v.string(), 'An array is required'),
    v.minLength(1, 'At least 1 option must be selected')
  )
});