import * as v from 'valibot';

const maxNDecimals = (val: number, n: number) => {
    const str = val.toString();
    if (!str.includes('.')) return true;
    return str.split('.')[1]!.length <= n;
};

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
            v.minValue(1, 'Target amount must be greater than 0'),
            v.check((val) => maxNDecimals(val, 2), 'Target amount can have at most 2 decimal places')
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

export const sellerSchema = v.object({
    name: v.optional(
        v.pipe(
            v.nullish(v.string()),
            v.check((val) => typeof val !== 'string' || val.length <= 100, 'Seller name must be less than 100 characters')
        )
    ),
    description: v.optional(
        v.pipe(
            v.nullish(v.string()),
            v.check((val) => typeof val !== 'string' || val.length <= 500, 'Description must be less than 500 characters')
        )
    )
});

export const productDataSchema = v.object({
    name: v.pipe(
        v.string(),
        v.nonEmpty('Product name is required'),
        v.maxLength(200, 'Product name must be less than 200 characters')
    ),
    description: v.optional(
        v.pipe(
            v.string(),
            v.maxLength(500, 'Description must be less than 500 characters')
        )
    ),
    categoryIds: v.any()
});

export const formProductSchema = v.object({
    _uid: v.string(),
    id: v.optional(v.number()),
    name: v.pipe(
        v.string(), 
        v.nonEmpty('Product name is required')
    ),
    price: v.pipe(
        v.number(), 
        v.minValue(0.01, 'Price must be greater than 0'),
        v.check((val) => maxNDecimals(val, 2), 'Price can have at most 2 decimal places')
    ),
    quantity: v.pipe(
        v.number(), 
        v.minValue(0.01, 'Quantity must be greater than 0'),
        v.check((val) => maxNDecimals(val, 3), 'Quantity can have at most 3 decimal places')
    ),
    categoryIds: v.any()
});

export const receiptSchema = v.object({
    paymentDate: v.pipe(
        v.string(), 
        v.nonEmpty('Payment date is required'),
        v.isoDate('Payment date must be a valid date')
    ),
    sellerId: v.pipe(
        v.string(), 
        v.nonEmpty('Seller is required')
    ),
    products: v.pipe(
        v.array(formProductSchema), 
        v.minLength(1, 'At least one product is required')
    )
});